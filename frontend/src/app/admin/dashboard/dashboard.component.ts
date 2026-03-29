import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { RsvpService } from '../../core/services/rsvp.service';
import { FeaturesService } from '../../core/services/features.service';
import { RsvpSummary, ModulesDto } from '../../core/models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatButtonModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private rsvpService = inject(RsvpService);
  private featuresService = inject(FeaturesService);

  summary = signal<RsvpSummary | null>(null);
  modules = signal<ModulesDto | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.featuresService.getModules().subscribe({
      next: (m) => this.modules.set(m),
    });

    this.rsvpService.getSummary().subscribe({
      next: (s) => {
        this.summary.set(s);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.status === 503 ? 'RSVP module is disabled.' : 'Failed to load RSVP summary.');
        this.loading.set(false);
      },
    });
  }
}
