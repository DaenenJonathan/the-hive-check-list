import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ImportPageComponent } from './pages/import-page.component';

const routes: Routes = [{ path: '', component: ImportPageComponent }];

@NgModule({
  declarations: [ImportPageComponent],
  imports: [SharedModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class ImportsModule {}
