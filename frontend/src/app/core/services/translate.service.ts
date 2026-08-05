import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AppTranslateService {
  private translations: Record<string, unknown> = {};
  private lang$ = new BehaviorSubject<string>('fr');
  currentLang$ = this.lang$.asObservable();

  constructor(private http: HttpClient) {}

  async use(lang: string): Promise<void> {
    const data = await firstValueFrom(
      this.http.get<Record<string, unknown>>(`/assets/i18n/${lang}.json`)
    );
    this.translations = data;
    this.lang$.next(lang);
    localStorage.setItem('lang', lang);
  }

  instant(key: string, params?: Record<string, unknown>): string {
    const value = this.getNestedValue(this.translations, key);
    if (!value) return key;
    if (params) {
      return value.replace(/\{\{(\w+)\}\}/g, (_: string, k: string) =>
        params[k] !== undefined ? String(params[k]) : `{{${k}}}`
      );
    }
    return value;
  }

  get currentLang(): string {
    return this.lang$.value;
  }

  private getNestedValue(obj: Record<string, unknown>, key: string): string {
    const parts = key.split('.');
    let current: unknown = obj;
    for (const part of parts) {
      if (typeof current !== 'object' || current === null) return '';
      current = (current as Record<string, unknown>)[part];
    }
    return typeof current === 'string' ? current : '';
  }
}
