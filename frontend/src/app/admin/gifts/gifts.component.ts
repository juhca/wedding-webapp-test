import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { GiftsService } from '../../core/services/gifts.service';
import { GiftDto } from '../../core/models/models';
import { GiftFormDialogComponent } from './gift-form-dialog.component';

@Component({
  selector: 'app-gifts',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTooltipModule,
    MatChipsModule,
    MatMenuModule,
  ],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({ height: '0px', minHeight: '0', overflow: 'hidden' })),
      state('expanded', style({ height: '*' })),
      transition('expanded <=> collapsed', animate('200ms ease')),
    ]),
  ],
  templateUrl: './gifts.component.html',
  styleUrl: './gifts.component.scss',
})
export class GiftsComponent implements OnInit {
  private giftsService = inject(GiftsService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  gifts = signal<GiftDto[]>([]);
  loading = signal(true);
  expandedGift = signal<GiftDto | null>(null);
  displayedColumns = ['expand', 'displayOrder', 'name', 'price', 'reservations', 'isVisible', 'actions'];

  ngOnInit(): void {
    this.loadGifts();
  }

  loadGifts(): void {
    this.loading.set(true);
    this.giftsService.getAll().subscribe({
      next: (gifts) => {
        this.gifts.set(gifts.sort((a, b) => a.displayOrder - b.displayOrder));
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load gifts', 'Close', { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  toggleGift(gift: GiftDto): void {
    this.expandedGift.set(this.expandedGift() === gift ? null : gift);
  }

  isExpanded(gift: GiftDto): boolean {
    return this.expandedGift() === gift;
  }

  openCreate(): void {
    const ref = this.dialog.open(GiftFormDialogComponent, { data: null, width: '500px' });
    ref.afterClosed().subscribe((result) => {
      if (result) {
        this.giftsService.create(result).subscribe({
          next: (g) => {
            this.gifts.update((gs) => [...gs, g].sort((a, b) => a.displayOrder - b.displayOrder));
            this.snackBar.open('Gift created', 'Close', { duration: 3000 });
          },
          error: () => this.snackBar.open('Failed to create gift', 'Close', { duration: 3000 }),
        });
      }
    });
  }

  openEdit(gift: GiftDto): void {
    const ref = this.dialog.open(GiftFormDialogComponent, { data: gift, width: '500px' });
    ref.afterClosed().subscribe((result) => {
      if (result) {
        this.giftsService.update(gift.id, result).subscribe({
          next: (updated) => {
            this.gifts.update((gs) => gs.map((g) => (g.id === updated.id ? updated : g)).sort((a, b) => a.displayOrder - b.displayOrder));
            this.snackBar.open('Gift updated', 'Close', { duration: 3000 });
          },
          error: () => this.snackBar.open('Failed to update gift', 'Close', { duration: 3000 }),
        });
      }
    });
  }

  delete(gift: GiftDto): void {
    if (!confirm(`Delete "${gift.name}"?`)) return;
    this.giftsService.delete(gift.id).subscribe({
      next: () => {
        this.gifts.update((gs) => gs.filter((g) => g.id !== gift.id));
        this.snackBar.open('Gift deleted', 'Close', { duration: 3000 });
      },
      error: (err) => this.snackBar.open(err.error?.message ?? 'Failed to delete', 'Close', { duration: 3000 }),
    });
  }

  importCsv(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    this.giftsService.importCsv(input.files[0]).subscribe({
      next: (result) => {
        this.snackBar.open(`Imported ${result.imported}, Skipped ${result.skipped}`, 'Close', { duration: 4000 });
        this.loadGifts();
      },
      error: () => this.snackBar.open('CSV import failed', 'Close', { duration: 3000 }),
    });
    input.value = '';
  }
}

