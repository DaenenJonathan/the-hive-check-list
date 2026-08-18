import { Injectable } from '@angular/core';
import { CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(_route: unknown, state: RouterStateSnapshot): boolean {
    if (!this.authService.isAuthenticated) {
      this.router.navigate(['/login']);
      return false;
    }
    if (this.authService.currentUser?.mustChangePassword && !state.url.startsWith('/change-password')) {
      this.router.navigate(['/change-password']);
      return false;
    }
    return true;
  }
}
