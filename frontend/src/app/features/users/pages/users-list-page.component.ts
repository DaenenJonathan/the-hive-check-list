import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { CreateUserResult, UserAdminDto } from '../models/user-admin.model';
import { UserAdminService } from '../services/user-admin.service';
import { UserRole } from '../../../core/models/user.model';
import { AgencyDto } from '../../agencies/models/agency.model';
import { AgencyService } from '../../agencies/services/agency.service';
import { BrandDto } from '../../brands/models/brand.model';
import { BrandService } from '../../brands/services/brand.service';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { AuthService } from '../../../core/auth/auth.service';

interface PasswordBanner {
  email: string;
  password: string;
  emailSent: boolean;
}

// A sibling control's own validators can't see role changes automatically - the component
// re-triggers them via updateValueAndValidity() whenever "role"/"agencyId" change (see
// wireRoleValidityTriggers). These read control.parent so they stay correct either way.
function isEmptyValue(value: unknown): boolean {
  return value == null || value === '' || (Array.isArray(value) && value.length === 0);
}

function requiredForRoles(roles: UserRole[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!roles.includes(control.parent?.get('role')?.value)) return null;
    return isEmptyValue(control.value) ? { required: true } : null;
  };
}

function emptyUnlessRoles(roles: UserRole[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (roles.includes(control.parent?.get('role')?.value)) return null;
    return isEmptyValue(control.value) ? null : { notAllowed: true };
  };
}

@Component({
  selector: 'app-users-list-page',
  templateUrl: './users-list-page.component.html',
  standalone: false
})
export class UsersListPageComponent implements OnInit {
  UserRole = UserRole;
  roles = Object.values(UserRole);
  users: UserAdminDto[] = [];
  agencies: AgencyDto[] = [];
  brands: BrandDto[] = [];
  loading = false;
  editingId: string | null = null;
  form: FormGroup;
  error = '';

  showCreateForm = false;
  createForm: FormGroup;
  createError = '';
  passwordBanner: PasswordBanner | null = null;

  constructor(
    private userAdminService: UserAdminService,
    private agencyService: AgencyService,
    private brandService: BrandService,
    private fb: FormBuilder,
    private confirmDialogService: ConfirmDialogService,
    private authService: AuthService
  ) {
    const agencyRequiredRoles = [UserRole.AgencyManager, UserRole.Manager];
    this.form = this.fb.group({
      role: ['', Validators.required],
      agencyId: ['', [requiredForRoles(agencyRequiredRoles), emptyUnlessRoles(agencyRequiredRoles)]],
      brandIds: [[] as string[], [requiredForRoles([UserRole.Manager]), emptyUnlessRoles([UserRole.Manager])]]
    });
    this.createForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      role: ['', Validators.required],
      agencyId: ['', [requiredForRoles(agencyRequiredRoles), emptyUnlessRoles(agencyRequiredRoles)]],
      brandIds: [[] as string[], [requiredForRoles([UserRole.Manager]), emptyUnlessRoles([UserRole.Manager])]]
    });
    this.wireRoleValidityTriggers(this.form);
    this.wireRoleValidityTriggers(this.createForm);
  }

  private wireRoleValidityTriggers(form: FormGroup): void {
    const agencyIdControl = form.get('agencyId')!;
    const brandIdsControl = form.get('brandIds')!;
    form.get('role')!.valueChanges.subscribe(() => {
      agencyIdControl.updateValueAndValidity();
      brandIdsControl.updateValueAndValidity();
    });
    // Changing the agency invalidates any previously-picked brands from the old agency - drop
    // them rather than silently keeping an inconsistent selection around.
    agencyIdControl.valueChanges.subscribe(() => {
      if (form.value.role === UserRole.Manager) brandIdsControl.setValue([]);
      brandIdsControl.updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    this.agencyService.getAll().subscribe({ next: data => { this.agencies = data; } });
    this.brandService.getAll().subscribe({ next: data => { this.brands = data; } });
    this.load();
  }

  load(): void {
    this.loading = true;
    this.userAdminService.getAll().subscribe({
      next: data => { this.users = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  startEdit(user: UserAdminDto): void {
    this.editingId = user.id;
    this.form.setValue({
      role: user.role,
      agencyId: user.agencyId ?? '',
      brandIds: user.brands.map(b => b.id)
    });
    this.error = '';
  }

  cancel(): void {
    this.editingId = null;
    this.form.reset({ role: '', agencyId: '', brandIds: [] });
    this.error = '';
  }

  get isAgencyFieldVisible(): boolean {
    const role = this.form.value.role;
    return role === UserRole.AgencyManager || role === UserRole.Manager;
  }

  get isManagerSelected(): boolean {
    return this.form.value.role === UserRole.Manager;
  }

  // Once a Manager's agency is chosen, only that agency's brands are selectable - mirrors the
  // "an AgencyManager only ever sees their own agency's brands" rule already enforced server-side.
  brandsForAgency(form: FormGroup): BrandDto[] {
    const agencyId = form.value.agencyId;
    if (!agencyId) return [];
    return this.brands.filter(b => b.agencyId === agencyId);
  }

  isBrandSelected(form: FormGroup, brandId: string): boolean {
    return (form.value.brandIds ?? []).includes(brandId);
  }

  toggleBrand(form: FormGroup, brandId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const current: string[] = form.value.brandIds ?? [];
    const next = checked ? [...current, brandId] : current.filter(id => id !== brandId);
    const control = form.get('brandIds')!;
    control.setValue(next);
    control.markAsDirty();
  }

  allBrandsSelected(form: FormGroup): boolean {
    const available = this.brandsForAgency(form);
    const current: string[] = form.value.brandIds ?? [];
    return available.length > 0 && current.length === available.length;
  }

  brandNames(user: UserAdminDto): string {
    return user.brands.map(b => b.name).join(', ');
  }

  toggleAllBrands(form: FormGroup): void {
    const control = form.get('brandIds')!;
    control.setValue(this.allBrandsSelected(form) ? [] : this.brandsForAgency(form).map(b => b.id));
    control.markAsDirty();
  }

  submit(user: UserAdminDto): void {
    if (this.form.invalid) return;
    const role = this.form.value.role as UserRole;
    const agencyId = this.isAgencyFieldVisible ? (this.form.value.agencyId || null) : null;
    const brandIds = role === UserRole.Manager ? (this.form.value.brandIds as string[]) : [];

    this.userAdminService.updateRole(user.id, { role, agencyId, brandIds }).subscribe({
      next: () => { this.cancel(); this.load(); },
      error: () => { this.error = 'COMMON.ERROR_GENERIC'; }
    });
  }

  get isCreateAgencyFieldVisible(): boolean {
    const role = this.createForm.value.role;
    return role === UserRole.AgencyManager || role === UserRole.Manager;
  }

  get isCreateManagerSelected(): boolean {
    return this.createForm.value.role === UserRole.Manager;
  }

  startCreate(): void {
    this.createForm.reset({ email: '', firstName: '', lastName: '', role: '', agencyId: '', brandIds: [] });
    this.createError = '';
    this.showCreateForm = true;
  }

  cancelCreate(): void {
    this.showCreateForm = false;
    this.createForm.reset({ email: '', firstName: '', lastName: '', role: '', agencyId: '', brandIds: [] });
    this.createError = '';
  }

  submitCreate(): void {
    if (this.createForm.invalid) return;
    const { email, firstName, lastName, role, agencyId, brandIds } = this.createForm.value;
    const resolvedAgencyId = this.isCreateAgencyFieldVisible ? (agencyId || null) : null;
    const resolvedBrandIds = role === UserRole.Manager ? (brandIds as string[]) : [];

    this.userAdminService.create({ email, firstName, lastName, role, agencyId: resolvedAgencyId, brandIds: resolvedBrandIds }).subscribe({
      next: (result: CreateUserResult) => {
        this.cancelCreate();
        this.load();
        if (!result.emailSent && result.temporaryPassword) {
          this.passwordBanner = { email, password: result.temporaryPassword, emailSent: false };
        } else {
          this.passwordBanner = { email, password: '', emailSent: true };
        }
      },
      error: err => { this.createError = err.error?.errors?.[0] || 'COMMON.ERROR_GENERIC'; }
    });
  }

  async resetPassword(user: UserAdminDto): Promise<void> {
    const ok = await this.confirmDialogService.confirm({
      title: 'Réinitialiser le mot de passe',
      message: `Réinitialiser le mot de passe de ${user.firstName} ${user.lastName} ?`,
      confirmLabel: 'Réinitialiser',
      variant: 'primary'
    });
    if (!ok) return;

    this.userAdminService.resetPassword(user.id).subscribe({
      next: result => {
        this.passwordBanner = { email: user.email, password: result.temporaryPassword, emailSent: false };
      }
    });
  }

  isSelf(user: UserAdminDto): boolean {
    return user.id === this.authService.currentUser?.id;
  }

  async deleteUser(user: UserAdminDto): Promise<void> {
    const ok = await this.confirmDialogService.confirm({
      title: 'Supprimer l\'utilisateur',
      message: `Supprimer définitivement le compte de ${user.firstName} ${user.lastName} (${user.email}) ?`,
      confirmLabel: 'Supprimer',
      variant: 'danger'
    });
    if (!ok) return;

    this.userAdminService.deleteUser(user.id).subscribe({
      next: () => this.load(),
      error: () => { this.error = 'COMMON.ERROR_GENERIC'; }
    });
  }

  dismissPasswordBanner(): void {
    this.passwordBanner = null;
  }

  copyPassword(): void {
    if (this.passwordBanner?.password) {
      navigator.clipboard.writeText(this.passwordBanner.password);
    }
  }
}
