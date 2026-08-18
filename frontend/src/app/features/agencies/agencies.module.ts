import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { AgenciesListPageComponent } from './pages/agencies-list-page.component';

const routes: Routes = [{ path: '', pathMatch: 'full', component: AgenciesListPageComponent }];

@NgModule({
  declarations: [AgenciesListPageComponent],
  imports: [SharedModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class AgenciesModule {}
