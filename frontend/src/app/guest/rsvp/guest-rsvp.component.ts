import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormArray, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { RsvpService } from '../../core/services/rsvp.service';
import { ReminderService } from '../../core/services/reminder.service';
import { RsvpDto, ReminderDto, ReminderUnit, AddReminderDto } from '../../core/models/models';

@Component({
  selector: 'app-guest-rsvp',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatRadioModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatChipsModule,
    MatSelectModule,
    MatCheckboxModule,
    TranslateModule,
  ],
  templateUrl: './guest-rsvp.component.html',
  styleUrl: './guest-rsvp.component.scss',
})
export class GuestRsvpComponent implements OnInit {
  private rsvpService = inject(RsvpService);
  private reminderService = inject(ReminderService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private translate = inject(TranslateService);

  existing = signal<RsvpDto | null>(null);
  loading = signal(true);
  saving = signal(false);
  submitted = signal(false);

  reminders = signal<ReminderDto[]>([]);
  addingReminder = signal(false);
  reminderFormOpen = signal(false);

  readonly ReminderUnit = ReminderUnit;
  readonly unitLabels = ['RSVP.DAYS', 'RSVP.WEEKS', 'RSVP.MONTHS'];

  unitLabel(unit: ReminderUnit): string {
    return this.translate.instant(this.unitLabels[unit]);
  }

  reminderForm = this.fb.nonNullable.group({
    value: [7, [Validators.required, Validators.min(1), Validators.max(365)]],
    unit: [ReminderUnit.Days, Validators.required],
    note: [''],
    sameEmail: [true],
    customEmail: ['', [Validators.email]],
  });

  form = this.fb.nonNullable.group({
    isAttending: [true, Validators.required],
    contactEmail: ['', [Validators.email]],
    dietaryRestrictions: [''],
    notes: [''],
    companions: this.fb.array([]),
  });

  get companions(): FormArray {
    return this.form.get('companions') as FormArray;
  }

  get maxCompanions(): number {
    return this.existing()?.maxCompanionsAllowed ?? 1;
  }

  get canAddCompanion(): boolean {
    return this.companions.length < this.maxCompanions;
  }

  ngOnInit(): void {
    this.rsvpService.getMyRsvp().subscribe({
      next: (rsvp) => {
        if (rsvp) {
          this.existing.set(rsvp);
          this.patchForm(rsvp);
          this.loadReminders();
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadReminders(): void {
    this.reminderService.getRsvpReminders().subscribe({
      next: (r) => this.reminders.set(r),
      error: () => {},
    });
  }

  private patchForm(rsvp: RsvpDto): void {
    this.form.patchValue({
      isAttending: rsvp.isAttending,
      contactEmail: rsvp.contactEmail ?? '',
      dietaryRestrictions: rsvp.dietaryRestrictions ?? '',
      notes: rsvp.notes ?? '',
    });
    this.companions.clear();
    rsvp.companions.forEach((c) => this.companions.push(this.buildCompanion(c.firstName, c.lastName, c.age, c.dietaryRestrictions)));
  }

  buildCompanion(firstName = '', lastName = '', age: number | undefined = undefined, dietary = ''): ReturnType<FormBuilder['group']> {
    return this.fb.group({
      firstName: [firstName, Validators.required],
      lastName: [lastName, Validators.required],
      age: [age ?? null as number | null],
      dietaryRestrictions: [dietary],
    });
  }

  addCompanion(): void {
    if (!this.canAddCompanion) return;
    this.companions.push(this.buildCompanion());
  }

  removeCompanion(i: number): void {
    this.companions.removeAt(i);
  }

  submit(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const val = this.form.getRawValue();

    this.rsvpService.createOrUpdate({
      isAttending: val.isAttending,
      contactEmail: val.contactEmail || undefined,
      dietaryRestrictions: val.dietaryRestrictions || undefined,
      notes: val.notes || undefined,
      companions: val.companions.map((c: any) => ({
        firstName: c.firstName,
        lastName: c.lastName,
        age: c.age ?? undefined,
        dietaryRestrictions: c.dietaryRestrictions || undefined,
      })),
    }).subscribe({
      next: (rsvp) => {
        this.existing.set(rsvp);
        this.saving.set(false);
        this.submitted.set(true);
        this.loadReminders();
        this.snackBar.open(
          rsvp.isAttending ? this.translate.instant('RSVP.SNACK_ATTENDING') : this.translate.instant('RSVP.SNACK_DECLINED'),
          this.translate.instant('COMMON.CLOSE'), { duration: 5000 }
        );
      },
      error: (err) => {
        this.saving.set(false);
        this.snackBar.open(err.error ?? this.translate.instant('RSVP.SNACK_ERROR'), this.translate.instant('COMMON.CLOSE'), { duration: 4000 });
      },
    });
  }

  addReminder(): void {
    if (this.reminderForm.invalid) return;
    this.addingReminder.set(true);
    const val = this.reminderForm.getRawValue();
    const contactEmail = this.form.getRawValue().contactEmail;

    let overrideEmail: string | undefined;
    if (val.sameEmail) {
      overrideEmail = contactEmail || undefined;
    } else {
      overrideEmail = val.customEmail || undefined;
    }

    const dto: AddReminderDto = {
      value: val.value,
      unit: val.unit,
      note: val.note || undefined,
      overrideEmail,
    };

    this.reminderService.addRsvpReminder(dto).subscribe({
      next: () => {
        this.loadReminders();
        this.addingReminder.set(false);
        this.reminderFormOpen.set(false);
        this.reminderForm.reset({ value: 7, unit: ReminderUnit.Days, note: '', sameEmail: true, customEmail: '' });
        this.snackBar.open(this.translate.instant('RSVP.SNACK_REMINDER_SET'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 });
      },
      error: (err) => {
        this.addingReminder.set(false);
        this.snackBar.open(err.error ?? this.translate.instant('RSVP.SNACK_REMINDER_ERROR'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 });
      },
    });
  }

  deleteReminder(id: string): void {
    this.reminderService.deleteReminder(id).subscribe({
      next: () => this.loadReminders(),
      error: () => this.snackBar.open(this.translate.instant('RSVP.SNACK_REMINDER_DELETE_ERROR'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 }),
    });
  }
}
