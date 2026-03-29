import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { GiftDto } from '../../core/models/models';

@Component({
  selector: 'app-reserve-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatIconModule, TranslateModule],
  template: `
    <h2 mat-dialog-title>{{ 'DIALOG.RESERVE_TITLE' | translate: { name: data.name } }}</h2>
    <mat-dialog-content>
      @if (data.price) {
        <p class="price">€{{ data.price }}</p>
      }
      @if (data.purchaseLink) {
        <p><a [href]="data.purchaseLink" target="_blank" class="buy-link">
          <mat-icon>open_in_new</mat-icon> {{ 'DIALOG.VIEW_ON_STORE' | translate }}
        </a></p>
      }
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>{{ 'DIALOG.NOTES' | translate }}</mat-label>
        <textarea matInput [formControl]="notesCtrl" rows="2" [placeholder]="'DIALOG.NOTES_PLACEHOLDER' | translate"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>{{ 'DIALOG.BTN_CANCEL' | translate }}</button>
      <button mat-flat-button color="primary" (click)="confirm()">
        <mat-icon>bookmark_add</mat-icon> {{ 'DIALOG.BTN_RESERVE' | translate }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`.full-width { width: 100%; } .price { font-size: 1.4rem; font-weight: 700; color: #c9a84c; margin: 0 0 8px; } .buy-link { display: inline-flex; align-items: center; gap: 4px; color: #1565c0; font-size: 0.9rem; } mat-dialog-content { min-width: 320px; }`],
})
export class ReserveDialogComponent {
  data: GiftDto = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<ReserveDialogComponent>);
  notesCtrl = new FormBuilder().control('');

  confirm(): void {
    this.dialogRef.close(this.notesCtrl.value ?? '');
  }
}
