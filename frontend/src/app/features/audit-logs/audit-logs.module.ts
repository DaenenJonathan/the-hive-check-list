import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { AuditLogsPageComponent } from './pages/audit-logs-page.component';

const routes: Routes = [{ path: '', component: AuditLogsPageComponent }];

@NgModule({
  declarations: [AuditLogsPageComponent],
  imports: [SharedModule, RouterModule.forChild(routes)]
})
export class AuditLogsModule {}
