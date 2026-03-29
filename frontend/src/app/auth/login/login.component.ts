import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';
import { LanguageService, SUPPORTED_LANGS, SupportedLang } from '../../core/services/language.service';
import { UserRole } from '../../core/models/models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
    TranslateModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private translate = inject(TranslateService);
  langService = inject(LanguageService);

  supportedLangs = SUPPORTED_LANGS;
  langMenuOpen = false;

  guestForm = this.fb.nonNullable.group({
    accessCode: ['', Validators.required],
  });

  adminForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  guestLoading = false;
  adminLoading = false;
  hidePassword = true;

  setLang(code: SupportedLang): void {
    this.langService.setLang(code);
    this.langMenuOpen = false;
  }

  submitGuest(): void {
    if (this.guestForm.invalid) return;
    this.guestLoading = true;
    const { accessCode } = this.guestForm.getRawValue();

    this.authService.guestLogin(accessCode.trim()).subscribe({
      next: () => {
        const user = this.authService.currentUser();
        if (user?.role === UserRole.Admin) {
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.router.navigate(['/home']);
        }
      },
      error: () => {
        this.guestLoading = false;
        this.snackBar.open(
          this.translate.instant('LOGIN.ERR_INVALID_CODE'),
          this.translate.instant('COMMON.CLOSE'),
          { duration: 4000 }
        );
      },
    });
  }

  submitAdmin(): void {
    if (this.adminForm.invalid) return;
    this.adminLoading = true;
    const { email, password } = this.adminForm.getRawValue();

    this.authService.adminLogin(email, password).subscribe({
      next: () => this.router.navigate(['/admin/dashboard']),
      error: () => {
        this.adminLoading = false;
        this.snackBar.open(
          this.translate.instant('LOGIN.ERR_INVALID_ADMIN'),
          this.translate.instant('COMMON.CLOSE'),
          { duration: 4000 }
        );
      },
    });
  }
}
