import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

const routes: Routes = [
  { path: '', redirectTo: 'actions', pathMatch: 'full' },
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  { path: 'register', redirectTo: 'login/register' },
  {
    path: 'change-password',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/account/account.module').then(m => m.AccountModule)
  },
  {
    path: 'actions',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/actions/actions.module').then(m => m.ActionsModule)
  },
  {
    path: 'checklists',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/checklists/checklists.module').then(m => m.ChecklistsModule)
  },
  {
    path: 'imports',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/imports/imports.module').then(m => m.ImportsModule)
  },
  {
    path: 'audit/:actionId',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/audit-logs/audit-logs.module').then(m => m.AuditLogsModule)
  },
  {
    path: 'agencies',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
    loadChildren: () => import('./features/agencies/agencies.module').then(m => m.AgenciesModule)
  },
  {
    path: 'brands',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin', 'AgencyManager'] },
    loadChildren: () => import('./features/brands/brands.module').then(m => m.BrandsModule)
  },
  {
    path: 'users',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] },
    loadChildren: () => import('./features/users/users.module').then(m => m.UsersModule)
  },
  { path: '**', redirectTo: 'actions' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
