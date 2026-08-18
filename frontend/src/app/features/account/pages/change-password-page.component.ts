import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-change-password-page',
  templateUrl: './change-password-page.component.html',
  standalone: false
})
export class ChangePasswordPageComponent {
  form: FormGroup;
  loading = false;
  error = '';
  success = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6), this.passwordStrengthValidator]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordsMatchValidator });
  }

  get forced(): boolean {
    return !!this.authService.currentUser?.mustChangePassword;
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.authService.changePassword({
      currentPassword: this.form.value.currentPassword,
      newPassword: this.form.value.newPassword
    }).subscribe({
      next: () => {
        this.loading = false;
        this.success = true;
        setTimeout(() => this.router.navigate(['/actions']), 1500);
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error = err.status === 0
          ? 'LOGIN.ERROR_NETWORK'
          : (err.error?.errors?.[0] ?? 'CHANGE_PASSWORD.ERROR');
      }
    });
  }

  private passwordStrengthValidator(control: { value: string }): ValidationErrors | null {
    const value: string = control.value ?? '';
    const hasUppercase = /[A-Z]/.test(value);
    const hasDigit = /[0-9]/.test(value);
    return hasUppercase && hasDigit ? null : { weakPassword: true };
  }

  private passwordsMatchValidator(group: FormGroup): ValidationErrors | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }
}
