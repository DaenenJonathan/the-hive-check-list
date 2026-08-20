import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ActionDto, ActionPhotoKind, ActionReturnItemDto } from '../models/action.model';
import { ActionService } from '../services/action.service';
import { ChecklistService } from '../../checklists/services/checklist.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-action-returns-page',
  templateUrl: './action-returns-page.component.html',
  standalone: false
})
export class ActionReturnsPageComponent implements OnInit {
  actionId = '';
  action: ActionDto | null = null;
  items: ActionReturnItemDto[] = [];
  loading = false;
  savingId: string | null = null;
  validating = false;
  returnValidated = false;
  returnValidatedAt: string | null = null;
  sent = false;
  sentAt: string | null = null;
  materialPhotoPath: string | null = null;
  consumablesPhotoPath: string | null = null;
  uploadingPhoto: ActionPhotoKind | null = null;

  constructor(
    private route: ActivatedRoute,
    private actionService: ActionService,
    private checklistService: ChecklistService,
    private location: Location,
    public authService: AuthService
  ) {}

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    this.actionId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.actionService.getAll().subscribe(actions => {
      this.action = actions.find(a => a.id === this.actionId) ?? null;
    });
    this.actionService.getReturns(this.actionId).subscribe({
      next: data => {
        this.items = data.items;
        this.sent = data.sent;
        this.sentAt = data.sentAt;
        this.returnValidated = data.returnValidated;
        this.returnValidatedAt = data.returnValidatedAt;
        this.materialPhotoPath = data.materialPhotoPath;
        this.consumablesPhotoPath = data.consumablesPhotoPath;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  saveReturn(item: ActionReturnItemDto, quantityReturned: number): void {
    if (quantityReturned < 0) return;
    this.savingId = item.itemId;
    this.checklistService.updateItemReturn(item.itemId, quantityReturned).subscribe({
      next: () => { item.quantityReturned = quantityReturned; this.savingId = null; },
      error: () => { this.savingId = null; }
    });
  }

  uploadPhoto(kind: ActionPhotoKind, event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    this.uploadingPhoto = kind;
    this.actionService.uploadActionPhoto(this.actionId, kind, input.files[0]).subscribe({
      next: res => {
        if (kind === 'material') this.materialPhotoPath = res.photoPath;
        else this.consumablesPhotoPath = res.photoPath;
        this.uploadingPhoto = null;
      },
      error: () => { this.uploadingPhoto = null; }
    });
  }

  get photosComplete(): boolean {
    return !!this.materialPhotoPath && !!this.consumablesPhotoPath;
  }

  validateReturns(): void {
    this.validating = true;
    this.actionService.validateReturns(this.actionId).subscribe({
      next: () => {
        this.returnValidated = true;
        this.returnValidatedAt = new Date().toISOString();
        this.validating = false;
      },
      error: () => { this.validating = false; }
    });
  }

  returnDifference(item: ActionReturnItemDto): number {
    return item.quantityPrepared - (item.quantityReturned ?? 0);
  }

  get checklistGroups(): { checklistName: string; items: ActionReturnItemDto[] }[] {
    const map = new Map<string, ActionReturnItemDto[]>();
    for (const i of this.items) {
      if (!map.has(i.checklistName)) map.set(i.checklistName, []);
      map.get(i.checklistName)!.push(i);
    }
    return Array.from(map.entries()).map(([checklistName, items]) => ({ checklistName, items }));
  }
}
