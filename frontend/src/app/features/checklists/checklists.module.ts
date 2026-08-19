import { NgModule } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SharedModule } from '../../shared/shared.module';
import { ChecklistsListPageComponent } from './pages/checklists-list-page.component';
import { ChecklistDetailPageComponent } from './pages/checklist-detail-page.component';

const routes: Routes = [
  { path: '', component: ChecklistsListPageComponent },
  { path: ':id', component: ChecklistDetailPageComponent }
];

@NgModule({
  declarations: [ChecklistsListPageComponent, ChecklistDetailPageComponent],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    DragDropModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    RouterModule.forChild(routes)
  ]
})
export class ChecklistsModule {}
