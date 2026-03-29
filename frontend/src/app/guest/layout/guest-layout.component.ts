import { Component, inject, OnInit, signal, HostListener, ElementRef } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';
import { FeaturesService } from '../../core/services/features.service';
import { LanguageService, SUPPORTED_LANGS, SupportedLang } from '../../core/services/language.service';
import { ModulesDto } from '../../core/models/models';

@Component({
  selector: 'app-guest-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatButtonModule,
    MatIconModule,
    TranslateModule,
  ],
  templateUrl: './guest-layout.component.html',
  styleUrl: './guest-layout.component.scss',
})
export class GuestLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private featuresService = inject(FeaturesService);
  private router = inject(Router);
  private el = inject(ElementRef);
  langService = inject(LanguageService);

  currentUser = this.authService.currentUser;
  modules = signal<ModulesDto | null>(null);
  mobileMenuOpen = signal(false);
  accountOpen = signal(false);
  langMenuOpen = signal(false);

  supportedLangs = SUPPORTED_LANGS;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const host = this.el.nativeElement;
    if (!host.querySelector('.goo-account')?.contains(event.target)) {
      this.accountOpen.set(false);
    }
    if (!host.querySelector('.lang-switcher')?.contains(event.target)) {
      this.langMenuOpen.set(false);
    }
  }

  ngOnInit(): void {
    this.featuresService.getModules().subscribe({
      next: (m) => this.modules.set(m),
    });
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
    if (this.mobileMenuOpen()) this.accountOpen.set(false);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  toggleAccount(): void {
    this.accountOpen.update((v) => !v);
  }

  toggleLangMenu(): void {
    this.langMenuOpen.update((v) => !v);
  }

  setLang(code: SupportedLang): void {
    this.langService.setLang(code);
    this.langMenuOpen.set(false);
  }

  logout(): void {
    this.accountOpen.set(false);
    this.authService.logout();
    this.router.navigate(['/home']);
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
