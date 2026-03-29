import { Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export type SupportedLang = 'en' | 'sl' | 'es' | 'de';

export const SUPPORTED_LANGS: { code: SupportedLang; label: string; flag: string }[] = [
  { code: 'sl', label: 'SL', flag: '🇸🇮' },
  { code: 'en', label: 'EN', flag: '🇬🇧' },
  { code: 'es', label: 'ES', flag: '🇦🇷' },
  { code: 'de', label: 'DE', flag: '🇩🇪' },
];

const STORAGE_KEY = 'wedding_lang';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  currentLang = signal<SupportedLang>('en');

  constructor(private translate: TranslateService) {
    const saved = localStorage.getItem(STORAGE_KEY) as SupportedLang | null;
    const browser = navigator.language.slice(0, 2) as SupportedLang;
    const supported = SUPPORTED_LANGS.map((l) => l.code);
    const initial: SupportedLang = saved ?? (supported.includes(browser) ? browser : 'en');

    translate.addLangs(supported);
    translate.setDefaultLang('en');
    translate.use(initial);
    this.currentLang.set(initial);
  }

  setLang(lang: SupportedLang): void {
    this.translate.use(lang);
    this.currentLang.set(lang);
    localStorage.setItem(STORAGE_KEY, lang);
  }

  getLangMeta(code: SupportedLang) {
    return SUPPORTED_LANGS.find((l) => l.code === code)!;
  }
}
