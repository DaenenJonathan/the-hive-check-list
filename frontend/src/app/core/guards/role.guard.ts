import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const requiredRoles: string[] = route.data['roles'] ?? [];
    if (requiredRoles.length === 0) return true;
    if (this.authService.hasRole(...requiredRoles)) return true;
    this.router.navigate(['/forbidden']);
    return false;
  }
}
