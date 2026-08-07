import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActionDto, ActionStatus } from '../models/action.model';
import { ActionService } from '../services/action.service';
import { AuthService } from '../../../core/auth/auth.service';
import { ChecklistDto } from '../../checklists/models/checklist.model';
import { ChecklistService } from '../../checklists/services/checklist.service';

type SortDirection = 'asc' | 'desc';

@Component({
  selector: 'app-actions-list-page',
  templateUrl: './actions-list-page.component.html',
  standalone: false
})
export class ActionsListPageComponent implements OnInit {
  ActionStatus = ActionStatus;
  actions: ActionDto[] = [];
  loading = false;
  showForm = false;
  editingId: string | null = null;
  form: FormGroup;

  sortDirection: SortDirection = 'desc';
  filterFrom: Date | null = null;
  filterTo: Date | null = null;
  filterClient: string | null = null;
  templates: ChecklistDto[] = [];

  constructor(
    private actionService: ActionService,
    private checklistService: ChecklistService,
    private fb: FormBuilder,
    public authService: AuthService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      client: ['', [Validators.required, Validators.maxLength(200)]],
      plannedDate: ['', Validators.required],
      plannedDepartureTime: [''],
      plannedReturnTime: [''],
      description: [''],
      templateChecklistId: ['']
    });
  }

  ngOnInit(): void {
    this.load();
    this.checklistService.getAll().subscribe({
      next: data => { this.templates = data; }
    });
  }

  load(): void {
    this.loading = true;
    this.actionService.getAll().subscribe({
      next: data => { this.actions = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  startCreate(): void {
    this.editingId = null;
    this.form.reset();
    this.showForm = true;
  }

  startEdit(action: ActionDto): void {
    this.editingId = action.id;
    this.form.setValue({
      name: action.name,
      client: action.client,
      plannedDate: new Date(action.plannedDate),
      plannedDepartureTime: action.plannedDepartureTime ?? '',
      plannedReturnTime: action.plannedReturnTime ?? '',
      description: action.description ?? '',
      templateChecklistId: ''
    });
    this.showForm = true;
  }

  cancel(): void {
    this.showForm = false;
    this.editingId = null;
    this.form.reset();
  }

  submit(): void {
    if (this.form.invalid) return;

    const request = {
      ...this.form.value,
      // Send the calendar day the user actually picked as a bare date (no time/offset) - otherwise
      // serializing the picker's local-midnight Date shifts it to the previous day in UTC+ timezones.
      plannedDate: this.toDateOnlyString(this.form.value.plannedDate),
      plannedDepartureTime: this.toTimeSpanString(this.form.value.plannedDepartureTime),
      plannedReturnTime: this.toTimeSpanString(this.form.value.plannedReturnTime),
      templateChecklistId: this.editingId ? null : (this.form.value.templateChecklistId || null)
    };

    if (this.editingId) {
      this.actionService.update({ id: this.editingId, ...request }).subscribe({
        next: () => { this.cancel(); this.load(); }
      });
    } else {
      this.actionService.create(request).subscribe({
        next: () => { this.cancel(); this.load(); }
      });
    }
  }

  canMarkSent(action: ActionDto): boolean {
    if (action.sent || !action.isReadyToSend) return false;
    return this.startOfDay(action.plannedDate) <= this.startOfDay(new Date());
  }

  markSent(action: ActionDto): void {
    this.actionService.markAsSent(action.id).subscribe({ next: () => this.load() });
  }

  cancelAction(action: ActionDto): void {
    if (!confirm(`Annuler l'action "${action.name}" ?`)) return;
    this.actionService.cancelAction(action.id).subscribe({ next: () => this.load() });
  }

  reactivateAction(action: ActionDto): void {
    this.actionService.reactivateAction(action.id).subscribe({ next: () => this.load() });
  }

  deleteAction(action: ActionDto): void {
    if (!confirm(`Supprimer définitivement l'action "${action.name}" et toutes ses checklists ? Cette opération est irréversible.`)) return;
    this.actionService.deleteAction(action.id).subscribe({ next: () => this.load() });
  }

  toggleSortDirection(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
  }

  clearDateFilter(): void {
    this.filterFrom = null;
    this.filterTo = null;
  }

  get availableClients(): string[] {
    const clients = new Set(this.actions.map(a => a.client || 'Sans client'));
    return Array.from(clients).sort((a, b) => a.localeCompare(b));
  }

  progressPercent(a: ActionDto): number {
    return a.totalItems === 0 ? 0 : Math.round((a.preparedItems / a.totalItems) * 100);
  }

  get clientGroups(): { client: string; items: ActionDto[] }[] {
    // Default view hides actions whose planned date has already passed. As soon as the user picks
    // a manual "Du" date, or a client, that takes full control (including past dates, on purpose) -
    // picking a client should surface that client's whole history, with dates as an optional add-on.
    const base = (this.filterFrom || this.filterClient) ? this.actions : this.actions.filter(a => this.isUpcoming(a));
    const byDate = this.filterByDate(base);
    const filtered = this.filterClient ? byDate.filter(a => (a.client || 'Sans client') === this.filterClient) : byDate;
    const map = new Map<string, ActionDto[]>();
    for (const a of filtered) {
      const key = a.client || 'Sans client';
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(a);
    }
    const direction = this.sortDirection === 'asc' ? 1 : -1;
    return Array.from(map.entries()).map(([client, items]) => ({
      client,
      items: [...items].sort((a, b) =>
        direction * (new Date(a.plannedDate).getTime() - new Date(b.plannedDate).getTime())
      )
    }));
  }

  private filterByDate(items: ActionDto[]): ActionDto[] {
    if (!this.filterFrom) return items;

    const from = this.startOfDay(this.filterFrom);
    const to = this.filterTo ? this.startOfDay(this.filterTo) : from;

    return items.filter(a => {
      const t = this.startOfDay(a.plannedDate);
      return t >= from && t <= to;
    });
  }

  private isUpcoming(action: ActionDto): boolean {
    return this.startOfDay(action.plannedDate) >= this.startOfDay(new Date());
  }

  private toDateOnlyString(date: Date): string {
    const d = new Date(date);
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${month}-${day}`;
  }

  // The native <input type="time"> yields "HH:mm" (no seconds), but the backend's TimeSpan JSON
  // converter requires the full "HH:mm:ss" form.
  private toTimeSpanString(value: string | null): string | null {
    if (!value) return null;
    return value.split(':').length === 2 ? `${value}:00` : value;
  }

  private startOfDay(d: Date | string): number {
    const date = new Date(d);
    return new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
  }
}
