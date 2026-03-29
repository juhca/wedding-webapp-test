import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, NgIf } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { GiftsService } from '../../core/services/gifts.service';
import { GiftDto } from '../../core/models/models';
import { ReserveDialogComponent } from './reserve-dialog.component';

const EXAMPLE_GIFTS: GiftDto[] = [
  {
    id: 'ex1', name: 'KitchenAid Stand Mixer', displayOrder: 1, isVisible: true,
    description: 'Classic 5-quart stand mixer in Empire Red — perfect for baking together.',
    price: 399, reservationCount: 0, isFullyReserved: false, isReservedByMe: false,
    reservations: [], reservationStatus: 'Available',
    purchaseLink: 'https://www.kitchenaid.com',
  },
  {
    id: 'ex2', name: 'Honeymoon Fund', displayOrder: 2, isVisible: true,
    description: 'Help us create unforgettable memories on our honeymoon in Italy.',
    price: 50, reservationCount: 2, isFullyReserved: false, isReservedByMe: false,
    reservations: [], reservationStatus: 'Partially Reserved',
  },
  {
    id: 'ex3', name: 'Dyson V15 Vacuum', displayOrder: 3, isVisible: true,
    description: 'Powerful cordless vacuum to keep our new home spotless.',
    price: 599, reservationCount: 0, isFullyReserved: false, isReservedByMe: false,
    reservations: [], reservationStatus: 'Available',
  },
  {
    id: 'ex4', name: 'Wine Cellar Starter Kit', displayOrder: 4, isVisible: true,
    description: '6 carefully selected bottles of fine wine from around the world.',
    price: 120, reservationCount: 1, maxReservations: 1, isFullyReserved: true, isReservedByMe: false,
    reservations: [], reservationStatus: 'Reserved',
  },
  {
    id: 'ex5', name: 'Nespresso Coffee Machine', displayOrder: 5, isVisible: true,
    description: 'Start every morning together with the perfect espresso.',
    price: 249, reservationCount: 0, isFullyReserved: false, isReservedByMe: false,
    reservations: [], reservationStatus: 'Available',
    purchaseLink: 'https://www.nespresso.com',
  },
  {
    id: 'ex6', name: 'Le Creuset Cookware Set', displayOrder: 6, isVisible: true,
    description: 'Iconic cast iron pots and pans in Volcanic Orange — built to last a lifetime.',
    price: 550, reservationCount: 0, isFullyReserved: false, isReservedByMe: false,
    reservations: [], reservationStatus: 'Available',
  },
];

@Component({
  selector: 'app-guest-gifts',
  standalone: true,
  imports: [
    CommonModule,
    NgIf,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatChipsModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    TranslateModule,
  ],  templateUrl: './guest-gifts.component.html',
  styleUrl: './guest-gifts.component.scss',
})
export class GuestGiftsComponent implements OnInit {
  private giftsService = inject(GiftsService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private translate = inject(TranslateService);

  gifts = signal<GiftDto[]>([]);
  myGifts = signal<GiftDto[]>([]);
  loading = signal(true);
  isExample = signal(false); // true when showing placeholder data

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.giftsService.getAll().subscribe({
      next: (gifts) => {
        const visible = gifts.filter((g) => g.isVisible).sort((a, b) => a.displayOrder - b.displayOrder);
        if (visible.length === 0) {
          this.gifts.set(EXAMPLE_GIFTS);
          this.isExample.set(true);
        } else {
          this.gifts.set(visible);
          this.isExample.set(false);
        }
        this.myGifts.set(gifts.filter((g) => g.isReservedByMe));
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open(this.translate.instant('GIFTS.SNACK_LOAD_ERROR'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  openReserve(gift: GiftDto): void {
    if (this.isExample()) {
      this.snackBar.open(this.translate.instant('GIFTS.SNACK_NOT_POPULATED'), this.translate.instant('COMMON.OK'), { duration: 3000 });
      return;
    }
    if (gift.isFullyReserved && !gift.isReservedByMe) return;

    if (gift.isReservedByMe) {
      if (!confirm(this.translate.instant('GIFTS.CONFIRM_UNRESERVE', { name: gift.name }))) return;
      this.giftsService.unreserve(gift.id).subscribe({
        next: () => {
          this.snackBar.open(this.translate.instant('GIFTS.SNACK_REMOVED'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 });
          this.load();
        },
        error: (err) => this.snackBar.open(err.error?.message ?? this.translate.instant('GIFTS.SNACK_FAILED'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 }),
      });
      return;
    }

    const ref = this.dialog.open(ReserveDialogComponent, { data: gift, width: '400px' });
    ref.afterClosed().subscribe((notes: string | undefined) => {
      if (notes === undefined) return;
      this.giftsService.reserve(gift.id, { notes: notes || undefined }).subscribe({
        next: () => {
          this.snackBar.open(this.translate.instant('GIFTS.SNACK_RESERVED', { name: gift.name }), this.translate.instant('COMMON.CLOSE'), { duration: 4000 });
          this.load();
        },
        error: (err) => this.snackBar.open(err.error ?? this.translate.instant('GIFTS.SNACK_RESERVE_ERROR'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 }),
      });
    });
  }
}
