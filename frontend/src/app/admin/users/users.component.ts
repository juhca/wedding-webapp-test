import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UsersService } from '../../core/services/users.service';
import { User, UserRole } from '../../core/models/models';
import { AddUserDialogComponent } from './add-user-dialog.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
})
export class UsersComponent implements OnInit {
  private usersService = inject(UsersService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  users = signal<User[]>([]);
  loading = signal(true);
  displayedColumns = ['name', 'email', 'role', 'accessCode', 'maxCompanions'];

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.usersService.getAll().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load users', 'Close', { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  openAddUser(): void {
    const ref = this.dialog.open(AddUserDialogComponent, { width: '400px' });
    ref.afterClosed().subscribe((result) => {
      if (result) {
        this.usersService.addUser(result).subscribe({
          next: (user) => {
            this.users.update((users) => [...users, user]);
            this.snackBar.open(`User ${user.firstName} created successfully`, 'Close', { duration: 3000 });
          },
          error: (err) => {
            const msg = err.error || 'Failed to create user';
            this.snackBar.open(msg, 'Close', { duration: 4000 });
          },
        });
      }
    });
  }

  getRoleLabel(role: UserRole): string {
    return { [UserRole.Admin]: 'Admin', [UserRole.FullExperience]: 'Full Experience', [UserRole.LimitedExperience]: 'Limited' }[role] ?? 'Unknown';
  }

  getRoleColor(role: UserRole): string {
    return { [UserRole.Admin]: 'warn', [UserRole.FullExperience]: 'primary', [UserRole.LimitedExperience]: 'accent' }[role] ?? '';
  }
}
