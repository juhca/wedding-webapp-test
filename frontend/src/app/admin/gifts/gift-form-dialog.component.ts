import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { GiftDto } from '../../core/models/models';

@Component({
  selector: 'app-gift-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ data ? 'Edit Gift' : 'Create Gift' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" />
          @if (form.get('name')?.invalid && form.get('name')?.touched) {
            <mat-error>Name is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="3"></textarea>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Price (optional)</mat-label>
          <input matInput formControlName="price" type="number" min="0" />
          <span matTextPrefix>€&nbsp;</span>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Image URL</mat-label>
          <input matInput formControlName="imageUrl" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Purchase Link</mat-label>
          <input matInput formControlName="purchaseLink" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Max Reservations (leave empty for unlimited)</mat-label>
          <input matInput formControlName="maxReservations" type="number" min="1" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Display Order</mat-label>
          <input matInput formControlName="displayOrder" type="number" />
        </mat-form-field>

        <mat-checkbox formControlName="isVisible">Visible to guests</mat-checkbox>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" (click)="submit()" [disabled]="form.invalid">
        {{ data ? 'Save Changes' : 'Create Gift' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-form { display: flex; flex-direction: column; padding-top: 8px; min-width: 380px; }
    .full-width { width: 100%; margin-bottom: 4px; }
  `],
})
export class GiftFormDialogComponent {
  data: GiftDto | null = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<GiftFormDialogComponent>);

  form = new FormBuilder().nonNullable.group({
    name: [this.data?.name ?? '', Validators.required],
    description: [this.data?.description ?? ''],
    price: [this.data?.price ?? null as number | null],
    imageUrl: [this.data?.imageUrl ?? ''],
    purchaseLink: [this.data?.purchaseLink ?? ''],
    maxReservations: [this.data?.maxReservations ?? null as number | null],
    displayOrder: [this.data?.displayOrder ?? 0, Validators.required],
    isVisible: [this.data?.isVisible ?? true],
  });

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.getRawValue();
    this.dialogRef.close({
      name: val.name,
      description: val.description || undefined,
      price: val.price ?? undefined,
      imageUrl: val.imageUrl || undefined,
      purchaseLink: val.purchaseLink || undefined,
      maxReservations: val.maxReservations ?? undefined,
      displayOrder: val.displayOrder,
      isVisible: val.isVisible,
    });
  }
}


