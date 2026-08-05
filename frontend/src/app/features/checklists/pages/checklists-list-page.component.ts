import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ChecklistDto } from '../models/checklist.model';
import { ChecklistService } from '../services/checklist.service';
import { ActionService } from '../../actions/services/action.service';
import { ActionDto } from '../../actions/models/action.model';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-checklists-list-page',
  templateUrl: './checklists-list-page.component.html',
  standalone: false
})
export class ChecklistsListPageComponent implements OnInit {
  checklists: ChecklistDto[] = [];
  actions: ActionDto[] = [];
  loading = false;
  showForm = false;
  brandActionId: string | null = null;
  form: FormGroup;

  constructor(
    private checklistService: ChecklistService,
    private actionService: ActionService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    public authService: AuthService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      brandActionId: ['', Validators.required],
      eventDate: [null]
    });
  }

  ngOnInit(): void {
    this.brandActionId = this.route.snapshot.queryParamMap.get('brandActionId');
    this.load();
    if (this.authService.hasRole('Admin', 'Manager')) {
      this.actionService.getAll().subscribe(data => {
        this.actions = data;
        if (this.brandActionId) this.form.patchValue({ brandActionId: this.brandActionId });
      });
    }
  }

  load(): void {
    this.loading = true;
    this.checklistService.getAll(this.brandActionId ?? undefined).subscribe({
      next: data => { this.checklists = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  create(): void {
    if (this.form.invalid) return;
    const { name, brandActionId, eventDate } = this.form.value;
    this.checklistService.create(name, brandActionId, eventDate).subscribe({
      next: id => this.router.navigate(['/checklists', id])
    });
  }

  progressPercent(c: ChecklistDto): number {
    return c.totalItems === 0 ? 0 : Math.round((c.preparedItems / c.totalItems) * 100);
  }

  get actionGroups(): { name: string; id: string; items: ChecklistDto[] }[] {
    const map = new Map<string, { name: string; id: string; items: ChecklistDto[] }>();
    for (const c of this.checklists) {
      if (!map.has(c.brandActionId)) {
        map.set(c.brandActionId, { name: c.brandActionName ?? 'Sans action', id: c.brandActionId, items: [] });
      }
      map.get(c.brandActionId)!.items.push(c);
    }
    return Array.from(map.values());
  }
}
