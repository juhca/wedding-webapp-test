import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TranslateModule } from '@ngx-translate/core';
import { WeddingInfoService } from '../../core/services/wedding-info.service';
import { WeddingInfoDto } from '../../core/models/models';

interface TimelineItem { key: string; image: string; }
interface CarouselSlide { image: string; captionKey: string; }

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, TranslateModule],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss',
})
export class AboutComponent implements OnInit, OnDestroy {
  private weddingService = inject(WeddingInfoService);

  info = signal<WeddingInfoDto | null>(null);
  currentSlide = signal(0);
  brideExpanded = signal(false);
  groomExpanded = signal(false);
  private timer?: ReturnType<typeof setInterval>;

  timeline: TimelineItem[] = [
    { key: '1', image: 'https://picsum.photos/seed/tl-about1/440/320' },
    { key: '2', image: 'https://picsum.photos/seed/tl-about2/440/320' },
    { key: '3', image: 'https://picsum.photos/seed/tl-about3/440/320' },
    { key: '4', image: 'https://picsum.photos/seed/tl-about4/440/320' },
    { key: '5', image: 'https://picsum.photos/seed/tl-about5/440/320' },
  ];

  slides: CarouselSlide[] = [
    { image: 'https://picsum.photos/seed/car-about1/1200/560', captionKey: 'ABOUT.SLIDE_1' },
    { image: 'https://picsum.photos/seed/car-about2/1200/560', captionKey: 'ABOUT.SLIDE_2' },
    { image: 'https://picsum.photos/seed/car-about3/1200/560', captionKey: 'ABOUT.SLIDE_3' },
    { image: 'https://picsum.photos/seed/car-about4/1200/560', captionKey: 'ABOUT.SLIDE_4' },
    { image: 'https://picsum.photos/seed/car-about5/1200/560', captionKey: 'ABOUT.SLIDE_5' },
    { image: 'https://picsum.photos/seed/car-about6/1200/560', captionKey: 'ABOUT.SLIDE_6' },
  ];

  ngOnInit(): void {
    this.weddingService.get().subscribe({ next: (i) => this.info.set(i) });
    this.startTimer();
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  prev(): void {
    this.currentSlide.update((i) => (i === 0 ? this.slides.length - 1 : i - 1));
    this.resetTimer();
  }

  next(): void {
    this.currentSlide.update((i) => (i + 1) % this.slides.length);
    this.resetTimer();
  }

  goTo(i: number): void {
    this.currentSlide.set(i);
    this.resetTimer();
  }

  private startTimer(): void {
    this.timer = setInterval(() => {
      this.currentSlide.update((i) => (i + 1) % this.slides.length);
    }, 5000);
  }

  private stopTimer(): void { if (this.timer) clearInterval(this.timer); }
  private resetTimer(): void { this.stopTimer(); this.startTimer(); }
}
