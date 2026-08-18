import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BrandDto } from '../models/brand.model';
import { BrandService } from '../services/brand.service';
import { AgencyDto } from '../../agencies/models/agency.model';
import { AgencyService } from '../../agencies/services/agency.service';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-brands-list-page',
  templateUrl: './brands-list-page.component.html',
  standalone: false
})
export class BrandsListPageComponent implements OnInit {
  brands: BrandDto[] = [];
  agencies: AgencyDto[] = [];
  loading = false;
  showForm = false;
  editingId: string | null = null;
  form: FormGroup;
  error = '';
  filterAgencyId: string | null = null;

  constructor(
    private brandService: BrandService,
    private agencyService: AgencyService,
    private fb: FormBuilder,
    private confirmDialogService: ConfirmDialogService,
    public authService: AuthService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      agencyId: ['', Validators.required]
    });
  }

  get isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }

  ngOnInit(): void {
    this.agencyService.getAll().subscribe({ next: data => { this.agencies = data; } });
    this.load();
  }

  load(): void {
    this.loading = true;
    this.brandService.getAll(this.filterAgencyId).subscribe({
      next: data => { this.brands = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  startCreate(): void {
    this.editingId = null;
    const ownAgencyId = this.authService.currentUser?.agencyId ?? '';
    this.form.reset({ name: '', agencyId: this.isAdmin ? '' : ownAgencyId });
    this.error = '';
    this.showForm = true;
  }

  startEdit(brand: BrandDto): void {
    this.editingId = brand.id;
    this.form.setValue({ name: brand.name, agencyId: brand.agencyId });
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
    const request = { name: this.form.value.name, agencyId: this.form.value.agencyId };

    if (this.editingId) {
      this.brandService.update({ id: this.editingId, ...request }).subscribe({
        next: () => { this.cancel(); this.load(); },
        error: () => { this.error = 'COMMON.ERROR_GENERIC'; }
      });
    } else {
      this.brandService.create(request).subscribe({
        next: () => { this.cancel(); this.load(); },
        error: () => { this.error = 'COMMON.ERROR_GENERIC'; }
      });
    }
  }

  async delete(brand: BrandDto): Promise<void> {
    const ok = await this.confirmDialogService.confirm({
      title: 'Supprimer la marque',
      message: `Supprimer la marque "${brand.name}" ?`,
      confirmLabel: 'Supprimer',
      variant: 'danger'
    });
    if (!ok) return;
    this.brandService.delete(brand.id).subscribe({
      next: () => this.load(),
      error: err => { this.error = err.error?.errors?.[0] || 'COMMON.ERROR_GENERIC'; }
    });
  }
}
