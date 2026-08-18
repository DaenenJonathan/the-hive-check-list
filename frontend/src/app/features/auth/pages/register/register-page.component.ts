import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-register-page',
  templateUrl: './register-page.component.html',
  standalone: false
})
export class RegisterPageComponent {
  form: FormGroup;
  loading = false;
  submitted = false;
  error = '';

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.form = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      message: ['']
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.authService.requestAccount(this.form.value).subscribe({
      next: () => { this.loading = false; this.submitted = true; },
      error: (err: HttpErrorResponse) => {
        this.error = err.status === 0 ? 'LOGIN.ERROR_NETWORK' : 'REQUEST_ACCOUNT.ERROR';
        this.loading = false;
      }
    });
  }
}
