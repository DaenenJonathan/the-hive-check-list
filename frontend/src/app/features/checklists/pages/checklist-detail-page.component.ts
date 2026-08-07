import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { AddChecklistItemRequest, BrandActionStatus, ChecklistDetailDto, ChecklistItemDto, ChecklistItemStatus } from '../models/checklist.model';
import { ChecklistService } from '../services/checklist.service';
import { ChecklistRealtimeService } from '../../../core/services/checklist-realtime.service';
import { AuthService } from '../../../core/auth/auth.service';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { Subscription } from 'rxjs';

interface PendingItemChange {
  status: ChecklistItemStatus;
  quantityPrepared: number;
  remark: string | null;
}

@Component({
  selector: 'app-checklist-detail-page',
  templateUrl: './checklist-detail-page.component.html',
  standalone: false
})
export class ChecklistDetailPageComponent implements OnInit, OnDestroy {
  checklist: ChecklistDetailDto | null = null;
  loading = false;
  editMode = false;
  checklistId!: string;
  statuses = Object.keys(ChecklistItemStatus).filter(k => isNaN(Number(k)));
  ChecklistItemStatus = ChecklistItemStatus;

  newItem: AddChecklistItemRequest = this.emptyItem();
  showAddForm = false;
  enlargedItem: ChecklistItemDto | null = null;

  // Edits made in normal mode (status/quantité préparée/remarque) are kept here until the user
  // clicks "Enregistrer tout" - never sent one row at a time, and never lost to a realtime reload
  // triggered by another user (see load()).
  pendingChanges = new Map<string, PendingItemChange>();

  private realtimeSub?: Subscription;
  private checklistUpdatedSub?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private checklistService: ChecklistService,
    private realtimeService: ChecklistRealtimeService,
    private confirmDialogService: ConfirmDialogService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.checklistId = this.route.snapshot.paramMap.get('id')!;
    this.load();
    this.connectRealtime();
  }

  ngOnDestroy(): void {
    this.realtimeSub?.unsubscribe();
    this.checklistUpdatedSub?.unsubscribe();
    this.realtimeService.leaveChecklist(this.checklistId);
  }

  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.hasUnsavedChanges) {
      event.preventDefault();
      event.returnValue = '';
    }
  }

  load(): void {
    this.loading = true;
    this.checklistService.getById(this.checklistId).subscribe({
      next: data => {
        this.applyPendingChanges(data.items);
        this.checklist = data;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  // Re-applies any not-yet-saved local edits on top of freshly loaded data, so a realtime reload
  // triggered by someone else's change never wipes out this user's in-progress edits.
  private applyPendingChanges(items: ChecklistItemDto[]): void {
    if (this.pendingChanges.size === 0) return;
    for (const item of items) {
      const pending = this.pendingChanges.get(item.id);
      if (pending) {
        item.status = pending.status;
        item.quantityPrepared = pending.quantityPrepared;
        item.remark = pending.remark;
      }
    }
  }

  toggleEditMode(): void {
    this.editMode = !this.editMode;
    this.showAddForm = false;
  }

  get hasUnsavedChanges(): boolean {
    return this.pendingChanges.size > 0;
  }

  get isActionCancelled(): boolean {
    return this.checklist?.brandActionStatus === BrandActionStatus.Cancelled;
  }

  get pendingChangesCount(): number {
    return this.pendingChanges.size;
  }

  isDirty(item: ChecklistItemDto): boolean {
    return this.pendingChanges.has(item.id);
  }

  displayStatusLabel(item: ChecklistItemDto): string {
    return this.statusLabel(this.pendingChanges.get(item.id)?.status ?? item.status);
  }

  displayQuantityPrepared(item: ChecklistItemDto): number {
    return this.pendingChanges.get(item.id)?.quantityPrepared ?? item.quantityPrepared;
  }

  displayRemark(item: ChecklistItemDto): string {
    return this.pendingChanges.get(item.id)?.remark ?? item.remark ?? '';
  }

  onStatusFieldChange(item: ChecklistItemDto, value: string): void {
    this.getOrCreatePending(item).status = ChecklistItemStatus[value as keyof typeof ChecklistItemStatus];
  }

  onQuantityFieldChange(item: ChecklistItemDto, value: number): void {
    this.getOrCreatePending(item).quantityPrepared = value;
  }

  onRemarkFieldChange(item: ChecklistItemDto, value: string): void {
    this.getOrCreatePending(item).remark = value || null;
  }

  saveAll(): void {
    if (this.pendingChanges.size === 0) return;
    const items = Array.from(this.pendingChanges.entries()).map(([itemId, change]) => ({
      itemId,
      status: change.status,
      quantityPrepared: change.quantityPrepared,
      remark: change.remark
    }));
    this.checklistService.updateItemsStatusBatch(this.checklistId, items).subscribe({
      next: () => { this.pendingChanges.clear(); this.load(); }
    });
  }

  private getOrCreatePending(item: ChecklistItemDto): PendingItemChange {
    let pending = this.pendingChanges.get(item.id);
    if (!pending) {
      pending = { status: item.status, quantityPrepared: item.quantityPrepared, remark: item.remark };
      this.pendingChanges.set(item.id, pending);
    }
    return pending;
  }

  saveDetails(item: ChecklistItemDto, name: string, qty: number, category: string, notes: string, location: string): void {
    this.checklistService.updateItemDetails(item.id, {
      checklistId: this.checklistId,
      materialName: name.trim(),
      quantityRequested: qty,
      category: category.trim() || null,
      notes: notes.trim() || null,
      location: location.trim() || null
    }).subscribe({ next: () => this.load() });
  }

  async deleteItem(item: ChecklistItemDto): Promise<void> {
    const ok = await this.confirmDialogService.confirm({
      title: 'Supprimer l\'article',
      message: `Supprimer "${item.materialName}" ?`,
      confirmLabel: 'Supprimer',
      variant: 'danger'
    });
    if (!ok) return;
    this.checklistService.deleteItem(item.id).subscribe({ next: () => this.load() });
  }

  markReady(item: ChecklistItemDto): void {
    this.quickUpdateStatus(item, ChecklistItemStatus.Prepared, item.quantityRequested);
  }

  markNotReady(item: ChecklistItemDto): void {
    this.quickUpdateStatus(item, ChecklistItemStatus.Missing, 0);
  }

  // Saves immediately, bypassing pendingChanges - unlike the dropdown/qty flow, one click = validated.
  private quickUpdateStatus(item: ChecklistItemDto, status: ChecklistItemStatus, quantityPrepared: number): void {
    this.checklistService.updateItemStatus(item.id, {
      itemId: item.id,
      status,
      quantityPrepared,
      remark: item.remark
    }).subscribe({
      next: () => {
        this.pendingChanges.delete(item.id);
        this.load();
      }
    });
  }

  addItem(): void {
    if (!this.newItem.materialName.trim() || this.newItem.quantityRequested <= 0) return;
    this.newItem.checklistId = this.checklistId;
    this.checklistService.addItem(this.newItem).subscribe({
      next: () => { this.newItem = this.emptyItem(); this.showAddForm = false; this.load(); }
    });
  }

  uploadImage(item: ChecklistItemDto, event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    this.checklistService.uploadItemImage(item.id, input.files[0]).subscribe({
      next: res => { item.imagePath = res.imagePath; }
    });
  }

  openEnlargedImage(item: ChecklistItemDto, event: Event): void {
    // The thumbnail sits inside the same <label> as the hidden file input (clicking the label
    // normally opens the file picker) - stop that so the click only opens the enlarged view.
    event.preventDefault();
    event.stopPropagation();
    if (item.imagePath) this.enlargedItem = item;
  }

  closeEnlargedImage(): void {
    this.enlargedItem = null;
  }

  @HostListener('window:keydown.escape')
  onEscapeKey(): void {
    this.closeEnlargedImage();
  }

  statusLabel(status: ChecklistItemStatus): string {
    return ChecklistItemStatus[status];
  }

  showCategoryHeader(item: ChecklistItemDto, index: number): boolean {
    if (!item.category) return false;
    if (index === 0) return true;
    return item.category !== this.checklist!.items[index - 1].category;
  }

  onDrop(event: CdkDragDrop<ChecklistItemDto[]>): void {
    const items = this.checklist!.items as ChecklistItemDto[];
    moveItemInArray(items, event.previousIndex, event.currentIndex);

    // Inherit category from the item above; keep original if dropped at top
    if (event.currentIndex > 0) {
      items[event.currentIndex].category = items[event.currentIndex - 1].category;
    }

    this.checklistService.reorderItems(
      this.checklistId,
      items.map((item, i) => ({ itemId: item.id, sortOrder: i, category: item.category ?? null }))
    ).subscribe();
  }

  private emptyItem(): AddChecklistItemRequest {
    return { checklistId: '', materialName: '', quantityRequested: 1, category: null, notes: null, location: null };
  }

  private async connectRealtime(): Promise<void> {
    await this.realtimeService.joinChecklist(this.checklistId);
    this.realtimeSub = this.realtimeService.itemUpdated$.subscribe(event => {
      if (event.checklistId === this.checklistId) this.load();
    });
    this.checklistUpdatedSub = this.realtimeService.checklistUpdated$.subscribe(event => {
      if (event.checklistId === this.checklistId) this.load();
    });
  }
}
