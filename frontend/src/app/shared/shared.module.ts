import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from './pipes/translate.pipe';
import { ImageUrlPipe } from './pipes/image-url.pipe';

@NgModule({
  declarations: [TranslatePipe, ImageUrlPipe],
  imports: [CommonModule],
  exports: [TranslatePipe, ImageUrlPipe, CommonModule]
})
export class SharedModule {}
