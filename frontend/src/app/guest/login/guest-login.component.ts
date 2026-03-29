import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../core/models/models';

@Component({
  selector: 'app-guest-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './guest-login.component.html',
  styleUrl: './guest-login.component.scss',
})
export class GuestLoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  form = this.fb.nonNullable.group({
    accessCode: ['', Validators.required],
  });

  loading = false;

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    const { accessCode } = this.form.getRawValue();

    this.authService.guestLogin(accessCode.trim()).subscribe({
      next: () => {
        const user = this.authService.currentUser();
        if (user?.role === UserRole.Admin) {
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.router.navigate(['/guest/home']);
        }
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Invalid access code. Please try again.', 'Close', { duration: 4000 });
      },
    });
  }
}
