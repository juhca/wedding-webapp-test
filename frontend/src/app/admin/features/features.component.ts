import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { FeaturesService } from '../../core/services/features.service';
import { ModulesDto } from '../../core/models/models';

@Component({
  selector: 'app-features',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule, MatChipsModule],
  templateUrl: './features.component.html',
  styleUrl: './features.component.scss',
})
export class FeaturesComponent implements OnInit {
  private featuresService = inject(FeaturesService);

  modules = signal<ModulesDto | null>(null);
  loading = signal(true);

  features: { key: keyof ModulesDto; label: string; icon: string; description: string }[] = [
    { key: 'rsvp', label: 'RSVP', icon: 'how_to_reg', description: 'Allows guests to submit their RSVP and dietary preferences.' },
    { key: 'gifts', label: 'Gift Registry', icon: 'card_giftcard', description: 'Allows guests to view and reserve gifts from the registry.' },
    { key: 'reminders', label: 'Reminders', icon: 'notifications', description: 'Allows guests to set email reminders for gifts and RSVP deadlines.' },
  ];

  ngOnInit(): void {
    this.featuresService.getModules().subscribe({
      next: (m) => {
        this.modules.set(m);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
