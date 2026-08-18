import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ChangePasswordPageComponent } from './pages/change-password-page.component';

const routes: Routes = [
  { path: '', component: ChangePasswordPageComponent }
];

@NgModule({
  declarations: [ChangePasswordPageComponent],
  imports: [SharedModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class AccountModule {}
