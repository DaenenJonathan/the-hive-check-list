import { Component, OnInit } from '@angular/core';
import { AppTranslateService } from './core/services/translate.service';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  languages = ['fr', 'nl', 'en'];
  currentLang = localStorage.getItem('lang') || 'fr';

  constructor(public translateService: AppTranslateService, public authService: AuthService) {}

  async ngOnInit(): Promise<void> {
    await this.translateService.use(this.currentLang);
  }

  async setLang(lang: string): Promise<void> {
    this.currentLang = lang;
    await this.translateService.use(lang);
  }
}
