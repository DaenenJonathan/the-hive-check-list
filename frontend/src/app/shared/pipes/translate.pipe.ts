import { Pipe, PipeTransform } from '@angular/core';
import { AppTranslateService } from '../../core/services/translate.service';

@Pipe({ name: 'translate', pure: false, standalone: false })
export class TranslatePipe implements PipeTransform {
  constructor(private translateService: AppTranslateService) {}

  transform(key: string, params?: Record<string, unknown>): string {
    return this.translateService.instant(key, params);
  }
}
