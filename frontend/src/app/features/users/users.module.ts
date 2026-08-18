import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { UsersListPageComponent } from './pages/users-list-page.component';

const routes: Routes = [{ path: '', pathMatch: 'full', component: UsersListPageComponent }];

@NgModule({
  declarations: [UsersListPageComponent],
  imports: [SharedModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class UsersModule {}
