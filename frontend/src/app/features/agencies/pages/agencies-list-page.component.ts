import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AgencyDto } from '../models/agency.model';
import { AgencyService } from '../services/agency.service';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';

@Component({
  selector: 'app-agencies-list-page',
  templateUrl: './agencies-list-page.component.html',
  standalone: false
})
export class AgenciesListPageComponent implements OnInit {
  agencies: AgencyDto[] = [];
  loading = false;
  showForm = false;
  editingId: string | null = null;
  form: FormGroup;
  error = '';

  constructor(
    private agencyService: AgencyService,
    private fb: FormBuilder,
    private confirmDialogService: ConfirmDialogService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      color: ['#2563EB', [Validators.required, Validators.pattern(/^#[0-9A-Fa-f]{6}$/)]]
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.agencyService.getAll().subscribe({
      next: data => { this.agencies = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  startCreate(): void {
    this.editingId = null;
    this.form.reset({ name: '', color: '#2563EB' });
    this.error = '';
    this.showForm = true;
  }

  startEdit(agency: AgencyDto): void {
    this.editingId = agency.id;
    this.form.setValue({ name: agency.name, color: agency.color });
    this.error = '';
    this.showForm = true;
  }

  cancel(): void {
    this.showForm = false;
    this.editingId = null;
    this.form.reset();
    this.error = '';
  }

  submit(): void {
    if (this.form.invalid) return;
    const request = { name: this.form.value.name, color: this.form.value.color };

    if (this.editingId) {
      this.agencyService.update({ id: this.editingId, ...request }).subscribe({
        next: () => { this.cancel(); this.load(); },
        error: () => { this.error = 'COMMON.ERROR_GENERIC'; }
      });
    } else {
      this.agencyService.create(request).subscribe({
        next: () => { this.cancel(); this.load(); },
        error: () => { this.error = 'COMMON.ERROR_GENERIC'; }
      });
    }
  }

  async delete(agency: AgencyDto): Promise<void> {
    const ok = await this.confirmDialogService.confirm({
      title: 'Supprimer l\'agence',
      message: `Supprimer l'agence "${agency.name}" ?`,
      confirmLabel: 'Supprimer',
      variant: 'danger'
    });
    if (!ok) return;
    this.agencyService.delete(agency.id).subscribe({
      next: () => this.load(),
      error: err => { this.error = err.error?.errors?.[0] || 'COMMON.ERROR_GENERIC'; }
    });
  }
}
