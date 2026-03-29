import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { TranslateModule } from '@ngx-translate/core';
import { WeddingInfoService } from '../../core/services/wedding-info.service';
import { FeaturesService } from '../../core/services/features.service';
import { AuthService } from '../../core/services/auth.service';
import { GiftsService } from '../../core/services/gifts.service';
import { WeddingInfoDto, LocationDto, UserRole, ModulesDto, GiftDto } from '../../core/models/models';

interface CountdownParts { d: number; h: number; m: number; s: number }

// Placeholder gifts shown on home page when user is not logged in / API has no data
const EXAMPLE_GIFTS: GiftDto[] = [
  { id: '1', name: 'KitchenAid Stand Mixer', description: 'Classic 5-quart stand mixer, perfect for baking.', price: 399, imageUrl: '', purchaseLink: '', reservationStatus: 'Available', isFullyReserved: false, isReservedByMe: false, reservationCount: 0, displayOrder: 1, isVisible: true, reservations: [] },
  { id: '2', name: 'Honeymoon Fund', description: 'Contribute to our dream honeymoon in Italy.', price: 50, reservationStatus: 'Available', isFullyReserved: false, isReservedByMe: false, reservationCount: 0, displayOrder: 2, isVisible: true, reservations: [] },
  { id: '3', name: 'Wine Cellar Starter Kit', description: '6 bottles of fine wine from around the world.', price: 120, reservationStatus: 'Available', isFullyReserved: false, isReservedByMe: false, reservationCount: 0, displayOrder: 3, isVisible: true, reservations: [] },
];

@Component({
  selector: 'app-guest-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    TranslateModule,
  ],
  templateUrl: './guest-home.component.html',
  styleUrl: './guest-home.component.scss',
})
export class GuestHomeComponent implements OnInit, OnDestroy {
  private weddingInfoService = inject(WeddingInfoService);
  private featuresService = inject(FeaturesService);
  private authService = inject(AuthService);
  private giftsService = inject(GiftsService);

  info = signal<WeddingInfoDto | null>(null);
  modules = signal<ModulesDto | null>(null);
  loading = signal(true);
  countdown = signal<CountdownParts | null>(null);
  previewGifts = signal<GiftDto[]>(EXAMPLE_GIFTS);

  private tickInterval?: ReturnType<typeof setInterval>;

  role = computed(() => this.authService.currentUser()?.role);
  isLoggedIn = this.authService.isLoggedIn;

  ngOnInit(): void {
    this.featuresService.getModules().subscribe({ next: (m) => this.modules.set(m) });
    this.weddingInfoService.get().subscribe({
      next: (info) => {
        this.info.set(info);
        this.loading.set(false);
        if (info.weddingDate) {
          this.startCountdown(new Date(info.weddingDate));
        }
      },
      error: () => this.loading.set(false),
    });
    // Try to load real gifts for preview; fallback to examples
    this.giftsService.getAll().subscribe({
      next: (gifts) => {
        if (gifts.length > 0) this.previewGifts.set(gifts.slice(0, 3));
      },
      error: () => {}, // not logged in — use example gifts
    });
  }

  ngOnDestroy(): void {
    clearInterval(this.tickInterval);
  }

  private startCountdown(target: Date): void {
    this.tick(target);
    this.tickInterval = setInterval(() => this.tick(target), 1000);
  }

  private tick(target: Date): void {
    const diff = target.getTime() - Date.now();
    if (diff <= 0) {
      this.countdown.set({ d: 0, h: 0, m: 0, s: 0 });
      clearInterval(this.tickInterval);
      return;
    }
    const d = Math.floor(diff / 86_400_000);
    const h = Math.floor((diff % 86_400_000) / 3_600_000);
    const m = Math.floor((diff % 3_600_000) / 60_000);
    const s = Math.floor((diff % 60_000) / 1000);
    this.countdown.set({ d, h, m, s });
  }

  pad(n: number): string {
    return n < 10 ? '0' + n : '' + n;
  }

  isWeddingDay(): boolean {
    const parts = this.countdown();
    return parts !== null && parts.d === 0 && parts.h === 0 && parts.m === 0 && parts.s === 0;
  }

  // ── Calendar export ─────────────────────────────────────────────────────────

  private calDate(): { ymd: string; nextYmd: string } | null {
    const raw = this.info()?.weddingDate;
    if (!raw) return null;
    const d = new Date(raw);
    const fmt = (dt: Date) =>
      `${dt.getFullYear()}${this.pad(dt.getMonth() + 1)}${this.pad(dt.getDate())}`;
    const next = new Date(d);
    next.setDate(next.getDate() + 1);
    return { ymd: fmt(d), nextYmd: fmt(next) };
  }

  addToGoogleCalendar(): void {
    const info = this.info();
    if (!info) return;
    const dates = this.calDate();
    if (!dates) return;
    const title = encodeURIComponent(info.weddingName || `${info.brideName} & ${info.groomName} Wedding`);
    const details = encodeURIComponent(info.weddingDescription || '');
    const location = encodeURIComponent(info.locationCivil?.address || info.locationChurch?.address || '');
    const url = `https://calendar.google.com/calendar/render?action=TEMPLATE&text=${title}&dates=${dates.ymd}/${dates.nextYmd}&details=${details}&location=${location}`;
    window.open(url, '_blank');
  }

  downloadIcs(label: string): void {
    const info = this.info();
    if (!info) return;
    const dates = this.calDate();
    if (!dates) return;
    const title = info.weddingName || `${info.brideName} & ${info.groomName} Wedding`;
    const location = info.locationCivil?.address || info.locationChurch?.address || '';
    const description = info.weddingDescription || '';
    const uid = `wedding-${dates.ymd}@wedding-app`;

    const ics = [
      'BEGIN:VCALENDAR',
      'VERSION:2.0',
      'PRODID:-//Wedding App//EN',
      'CALSCALE:GREGORIAN',
      'METHOD:PUBLISH',
      'BEGIN:VEVENT',
      `UID:${uid}`,
      `DTSTART;VALUE=DATE:${dates.ymd}`,
      `DTEND;VALUE=DATE:${dates.nextYmd}`,
      `SUMMARY:${title}`,
      description ? `DESCRIPTION:${description.replace(/\n/g, '\\n')}` : '',
      location ? `LOCATION:${location}` : '',
      'STATUS:CONFIRMED',
      `DTSTAMP:${new Date().toISOString().replace(/[-:]/g, '').split('.')[0]}Z`,
      'END:VEVENT',
      'END:VCALENDAR',
    ].filter(Boolean).join('\r\n');

    const blob = new Blob([ics], { type: 'text/calendar;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${label}.ics`;
    a.click();
    URL.revokeObjectURL(url);
  }

  hasFullAccess = computed(() => {
    const r = this.role();
    return r === UserRole.Admin || r === UserRole.FullExperience;
  });

  openMaps(location: LocationDto): void {
    const url = location.googleMapsUrl ?? `https://maps.google.com/?q=${location.latitude},${location.longitude}`;
    window.open(url, '_blank');
  }
}
