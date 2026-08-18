import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { BrandsListPageComponent } from './pages/brands-list-page.component';

const routes: Routes = [{ path: '', pathMatch: 'full', component: BrandsListPageComponent }];

@NgModule({
  declarations: [BrandsListPageComponent],
  imports: [SharedModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class BrandsModule {}
