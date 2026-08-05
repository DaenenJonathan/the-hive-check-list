import { NgModule } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SharedModule } from '../../shared/shared.module';
import { ChecklistsListPageComponent } from './pages/checklists-list-page.component';
import { ChecklistDetailPageComponent } from './pages/checklist-detail-page.component';

const routes: Routes = [
  { path: '', component: ChecklistsListPageComponent },
  { path: ':id', component: ChecklistDetailPageComponent }
];

@NgModule({
  declarations: [ChecklistsListPageComponent, ChecklistDetailPageComponent],
  imports: [SharedModule, ReactiveFormsModule, FormsModule, DragDropModule, RouterModule.forChild(routes)]
})
export class ChecklistsModule {}
