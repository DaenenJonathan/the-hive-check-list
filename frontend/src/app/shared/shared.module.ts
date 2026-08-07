import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from './pipes/translate.pipe';
import { ImageUrlPipe } from './pipes/image-url.pipe';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { NotificationBellComponent } from './components/notification-bell/notification-bell.component';

@NgModule({
  declarations: [TranslatePipe, ImageUrlPipe, ConfirmDialogComponent, NotificationBellComponent],
  imports: [CommonModule, RouterModule],
  exports: [TranslatePipe, ImageUrlPipe, CommonModule, ConfirmDialogComponent, NotificationBellComponent]
})
export class SharedModule {}
