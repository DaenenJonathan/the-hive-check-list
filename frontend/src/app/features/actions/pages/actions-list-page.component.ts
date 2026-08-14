import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActionDto, ActionStatus } from '../models/action.model';
import { ActionService } from '../services/action.service';
import { AuthService } from '../../../core/auth/auth.service';
import { ChecklistDto } from '../../checklists/models/checklist.model';
import { ChecklistService } from '../../checklists/services/checklist.service';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { ImportService } from '../../imports/services/import.service';

type SortDirection = 'asc' | 'desc';
type ViewMode = 'list' | 'day' | 'week';

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
  viewMode: ViewMode = 'list';
  referenceDate: Date = new Date();
  templates: ChecklistDto[] = [];
  importFile: File | null = null;
  importError = '';

  constructor(
    private actionService: ActionService,
    private checklistService: ChecklistService,
    private importService: ImportService,
    private fb: FormBuilder,
    private confirmDialogService: ConfirmDialogService,
    private router: Router,
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
    this.form.get('templateChecklistId')!.enable();
    this.importFile = null;
    this.importError = '';
    this.showForm = true;
  }

  // A template checklist and an imported file both produce the checklist for the new action, so
  // picking one disables the reactive form control for the other (a plain [disabled] binding on a
  // formControlName element gets fought/reset by Angular's reactive-forms directive, hence .disable()).
  onImportFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.importFile = input.files?.length ? input.files[0] : null;
    const templateControl = this.form.get('templateChecklistId')!;
    if (this.importFile) { templateControl.setValue(''); templateControl.disable(); }
    else templateControl.enable();
  }

  clearImportFile(fileInput: HTMLInputElement): void {
    this.importFile = null;
    fileInput.value = '';
    this.form.get('templateChecklistId')!.enable();
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
    this.form.get('templateChecklistId')!.enable();
    this.importFile = null;
    this.importError = '';
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
        next: newActionId => {
          if (this.importFile) {
            this.importService.confirm(this.importFile, newActionId).subscribe({
              next: result => { this.cancel(); this.router.navigate(['/checklists', result.checklistId]); },
              error: () => { this.importError = 'ACTIONS.IMPORT_CHECKLIST_ERROR'; this.load(); }
            });
            return;
          }

          this.cancel();
          if (!request.templateChecklistId) { this.load(); return; }

          // The template produced exactly one checklist for this brand-new action - jump straight to it.
          this.checklistService.getAll(newActionId).subscribe({
            next: checklists => {
              if (checklists.length === 1) this.router.navigate(['/checklists', checklists[0].id]);
              else this.load();
            }
          });
        }
      });
    }
  }

  // Clicking a card jumps straight to its checklist when there's exactly one (the common case) -
  // skips the intermediate checklists-list page the user would otherwise have to click through again.
  viewAction(action: ActionDto): void {
    if (action.singleChecklistId) {
      this.router.navigate(['/checklists', action.singleChecklistId]);
    } else {
      this.router.navigate(['/checklists'], { queryParams: { brandActionId: action.id } });
    }
  }

  canMarkSent(action: ActionDto): boolean {
    if (action.sent || !action.isReadyToSend) return false;
    return this.startOfDay(action.plannedDate) <= this.startOfDay(new Date());
  }

  markSent(action: ActionDto): void {
    this.actionService.markAsSent(action.id).subscribe({ next: () => this.load() });
  }

  async cancelAction(action: ActionDto): Promise<void> {
    const ok = await this.confirmDialogService.confirm({
      title: 'Annuler l\'action',
      message: `Annuler l'action "${action.name}" ?`,
      confirmLabel: 'Annuler l\'action',
      variant: 'primary'
    });
    if (!ok) return;
    this.actionService.cancelAction(action.id).subscribe({ next: () => this.load() });
  }

  async reactivateAction(action: ActionDto): Promise<void> {
    const ok = await this.confirmDialogService.confirm({
      title: 'Réactiver l\'action',
      message: `Réactiver l'action "${action.name}" ?`,
      confirmLabel: 'Réactiver',
      variant: 'success'
    });
    if (!ok) return;
    this.actionService.reactivateAction(action.id).subscribe({ next: () => this.load() });
  }

  async deleteAction(action: ActionDto): Promise<void> {
    const ok = await this.confirmDialogService.confirm({
      title: 'Supprimer l\'action',
      message: `Supprimer définitivement l'action "${action.name}" et toutes ses checklists ? Cette opération est irréversible.`,
      confirmLabel: 'Supprimer',
      variant: 'danger'
    });
    if (!ok) return;
    this.actionService.deleteAction(action.id).subscribe({ next: () => this.load() });
  }

  toggleSortDirection(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
  }

  clearDateFilter(): void {
    this.filterFrom = null;
    this.filterTo = null;
  }

  setViewMode(mode: ViewMode): void {
    this.viewMode = mode;
    // Day/Week always default to "now" when you switch into them - the same guarantee as loading
    // the page fresh - even though prev/next can then move away from it.
    if (mode !== 'list') this.referenceDate = new Date();
  }

  previousPeriod(): void {
    this.referenceDate = this.addDays(this.referenceDate, this.viewMode === 'week' ? -7 : -1);
  }

  nextPeriod(): void {
    this.referenceDate = this.addDays(this.referenceDate, this.viewMode === 'week' ? 7 : 1);
  }

  goToToday(): void {
    this.referenceDate = new Date();
  }

  // Angular's date pipe has no LOCALE_ID override in main.ts (always en-US), so weekday names must
  // come from our own i18n files, not date:'EEEE' - this just resolves which key to translate.
  weekdayLabelKey(date: Date): string {
    const keys = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'];
    return `ACTIONS.WEEKDAY_${keys[date.getDay()]}`;
  }

  get dayActions(): ActionDto[] {
    return this.actionsOnDate(this.referenceDate);
  }

  get weekStart(): Date {
    return this.startOfWeek(this.referenceDate);
  }

  get weekEnd(): Date {
    return this.addDays(this.weekStart, 6);
  }

  get weekColumns(): { date: Date; items: ActionDto[] }[] {
    const monday = this.weekStart;
    return Array.from({ length: 7 }, (_, i) => {
      const date = this.addDays(monday, i);
      return { date, items: this.actionsOnDate(date) };
    });
  }

  get availableClients(): string[] {
    const clients = new Set(this.actions.map(a => this.clientLabel(a)));
    return Array.from(clients).sort((a, b) => a.localeCompare(b));
  }

  progressPercent(a: ActionDto): number {
    return a.totalItems === 0 ? 0 : Math.round((a.preparedItems / a.totalItems) * 100);
  }

  // Backend TimeSpan comes over as "HH:mm:ss" - trim to "HH:mm" for display.
  formatTime(time: string | null): string {
    return time ? time.substring(0, 5) : '';
  }

  sendStatusClass(action: ActionDto): string {
    if (action.sent) return 'status-icon-success';
    if (action.isReadyToSend) return 'status-icon-warn';
    return 'status-icon-muted';
  }

  returnStatusClass(action: ActionDto): string {
    if (action.returnValidated) return 'status-icon-success';
    if (action.sent) return 'status-icon-warn';
    return 'status-icon-muted';
  }

  get clientGroups(): { client: string; items: ActionDto[] }[] {
    // Default view hides actions whose planned date has already passed. As soon as the user picks
    // a manual "Du" date, or a client, that takes full control (including past dates, on purpose) -
    // picking a client should surface that client's whole history, with dates as an optional add-on.
    const base = (this.filterFrom || this.filterClient) ? this.actions : this.actions.filter(a => this.isUpcoming(a));
    const byDate = this.filterByDate(base);
    const filtered = this.applyClientFilter(byDate);
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

  private actionsOnDate(date: Date): ActionDto[] {
    const target = this.startOfDay(date);
    const sameDay = this.actions.filter(a => this.startOfDay(a.plannedDate) === target);
    return this.sortByTime(this.applyClientFilter(sameDay));
  }

  private applyClientFilter(items: ActionDto[]): ActionDto[] {
    return this.filterClient ? items.filter(a => this.clientLabel(a) === this.filterClient) : items;
  }

  private clientLabel(a: ActionDto): string {
    return a.client || 'Sans client';
  }

  // Actions without a departure time aren't tied to a specific moment - list them first ("all day"),
  // then timed actions ascending. "" sorts before any "HH:mm:ss" string lexicographically, and the
  // backend's zero-padded HH:mm:ss format sorts correctly as a plain string (no Date parsing needed).
  private sortByTime(items: ActionDto[]): ActionDto[] {
    return [...items].sort((a, b) =>
      (a.plannedDepartureTime ?? '').localeCompare(b.plannedDepartureTime ?? '')
    );
  }

  private startOfWeek(d: Date): Date {
    const date = new Date(d.getFullYear(), d.getMonth(), d.getDate());
    const day = date.getDay(); // 0 = Sunday .. 6 = Saturday
    date.setDate(date.getDate() + (day === 0 ? -6 : 1 - day));
    return date;
  }

  private addDays(d: Date, n: number): Date {
    const date = new Date(d);
    date.setDate(date.getDate() + n);
    return date;
  }
}
