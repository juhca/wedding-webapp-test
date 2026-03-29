import { Component, Inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { UserRole } from '../../core/models/models';

@Component({
  selector: 'app-add-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>Add New User</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>First Name</mat-label>
          <input matInput formControlName="firstName" />
          @if (form.get('firstName')?.invalid && form.get('firstName')?.touched) {
            <mat-error>First name is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Last Name</mat-label>
          <input matInput formControlName="lastName" />
          @if (form.get('lastName')?.invalid && form.get('lastName')?.touched) {
            <mat-error>Last name is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email (optional)</mat-label>
          <input matInput formControlName="email" type="email" />
          @if (form.get('email')?.hasError('email')) {
            <mat-error>Enter a valid email</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Role</mat-label>
          <mat-select formControlName="role">
            <mat-option [value]="UserRole.Admin">Admin</mat-option>
            <mat-option [value]="UserRole.FullExperience">Full Experience</mat-option>
            <mat-option [value]="UserRole.LimitedExperience">Limited Experience</mat-option>
          </mat-select>
        </mat-form-field>

        @if (form.get('role')?.value === UserRole.Admin) {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Password</mat-label>
            <input matInput formControlName="password" type="password" />
            @if (form.get('password')?.invalid && form.get('password')?.touched) {
              <mat-error>Password is required for admin</mat-error>
            }
          </mat-form-field>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" (click)="submit()" [disabled]="form.invalid">
        Create User
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-form { display: flex; flex-direction: column; padding-top: 8px; min-width: 340px; }
    .full-width { width: 100%; margin-bottom: 4px; }
  `],
})
export class AddUserDialogComponent {
  UserRole = UserRole;

  form = new FormBuilder().nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', Validators.email],
    role: [UserRole.FullExperience, Validators.required],
    password: [''],
  });

  constructor(private dialogRef: MatDialogRef<AddUserDialogComponent>) {}

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.getRawValue();
    this.dialogRef.close({
      firstName: val.firstName,
      lastName: val.lastName,
      email: val.email || undefined,
      password: val.password || undefined,
      role: val.role,
    });
  }
}
