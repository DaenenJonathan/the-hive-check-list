import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { LoginPageComponent } from './pages/login/login-page.component';
import { RegisterPageComponent } from './pages/register/register-page.component';

const routes: Routes = [
  { path: '', component: LoginPageComponent },
  { path: 'register', component: RegisterPageComponent }
];

@NgModule({
  declarations: [LoginPageComponent, RegisterPageComponent],
  imports: [SharedModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class AuthModule {}
