import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { RsvpService } from '../../core/services/rsvp.service';
import { RsvpSummary, RsvpWithUser } from '../../core/models/models';

@Component({
  selector: 'app-rsvp',
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
    MatInputModule,
    MatFormFieldModule,
    MatChipsModule,
    MatTooltipModule,
  ],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({ height: '0px', minHeight: '0', overflow: 'hidden' })),
      state('expanded', style({ height: '*' })),
      transition('expanded <=> collapsed', animate('200ms ease')),
    ]),
  ],
  templateUrl: './rsvp.component.html',
  styleUrl: './rsvp.component.scss',
})
export class RsvpComponent implements OnInit {
  private rsvpService = inject(RsvpService);
  private snackBar = inject(MatSnackBar);

  summary = signal<RsvpSummary | null>(null);
  allRsvps = signal<RsvpWithUser[]>([]);
  loading = signal(true);
  exporting = signal(false);
  search = signal('');
  expandedRow = signal<RsvpWithUser | null>(null);

  displayedColumns = ['expand', 'name', 'attending', 'totalGuests', 'dietaryRestrictions', 'notes', 'respondedAt'];

  filteredRsvps = computed(() => {
    const q = this.search().toLowerCase();
    if (!q) return this.allRsvps();
    return this.allRsvps().filter(
      (r) =>
        r.firstName.toLowerCase().includes(q) ||
        r.lastName.toLowerCase().includes(q) ||
        r.email?.toLowerCase().includes(q)
    );
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.rsvpService.getSummary().subscribe({ next: (s) => this.summary.set(s) });
    this.rsvpService.getAll().subscribe({
      next: (rsvps) => {
        this.allRsvps.set(rsvps);
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load RSVPs', 'Close', { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  toggleRow(row: RsvpWithUser): void {
    this.expandedRow.set(this.expandedRow() === row ? null : row);
  }

  isExpanded(row: RsvpWithUser): boolean {
    return this.expandedRow() === row;
  }

  exportCsv(): void {
    this.exporting.set(true);
    this.rsvpService.exportCatering().subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `catering-export-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        URL.revokeObjectURL(url);
        this.exporting.set(false);
      },
      error: () => {
        this.snackBar.open('Export failed', 'Close', { duration: 3000 });
        this.exporting.set(false);
      },
    });
  }

  updateSearch(value: string): void {
    this.search.set(value);
  }
}
