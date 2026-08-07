import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: 'actions', pathMatch: 'full' },
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  { path: 'register', redirectTo: 'login/register' },
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
  { path: '**', redirectTo: 'actions' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
