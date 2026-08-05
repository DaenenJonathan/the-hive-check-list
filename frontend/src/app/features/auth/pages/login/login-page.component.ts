import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  standalone: false
})
export class LoginPageComponent {
  form: FormGroup;
  loading = false;
  error = '';

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.authService.login(this.form.value).subscribe({
      next: () => this.router.navigate(['/actions']),
      error: (err: HttpErrorResponse) => {
        this.error = err.status === 0 ? 'LOGIN.ERROR_NETWORK' : 'LOGIN.ERROR';
        this.loading = false;
      }
    });
  }
}
