import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from './pipes/translate.pipe';
import { ImageUrlPipe } from './pipes/image-url.pipe';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';

@NgModule({
  declarations: [TranslatePipe, ImageUrlPipe, ConfirmDialogComponent],
  imports: [CommonModule],
  exports: [TranslatePipe, ImageUrlPipe, CommonModule, ConfirmDialogComponent]
})
export class SharedModule {}
