import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DATE_LOCALE, MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ActionsListPageComponent } from './pages/actions-list-page.component';
import { ActionReturnsPageComponent } from './pages/action-returns-page.component';
import { ActionReturnsListPageComponent } from './pages/action-returns-list-page.component';

const routes: Routes = [
  { path: '', component: ActionsListPageComponent },
  { path: 'returns', component: ActionReturnsListPageComponent },
  { path: ':id/returns', component: ActionReturnsPageComponent }
];

@NgModule({
  declarations: [ActionsListPageComponent, ActionReturnsPageComponent, ActionReturnsListPageComponent],
  imports: [
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    RouterModule.forChild(routes)
  ],
  // The app has no global LOCALE_ID override (stays en-US - see the weekday/month label workaround
  // in ActionsListPageComponent), so the datepicker needs its own locale to render dd/mm/yyyy.
  providers: [{ provide: MAT_DATE_LOCALE, useValue: 'fr-BE' }]
})
export class ActionsModule {}
