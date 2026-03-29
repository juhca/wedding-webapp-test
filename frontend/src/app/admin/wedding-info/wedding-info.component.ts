import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { WeddingInfoService } from '../../core/services/wedding-info.service';
import { WeddingInfoDto, WeddingInfoUpdateDto } from '../../core/models/models';

@Component({
  selector: 'app-wedding-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
    MatExpansionModule,
  ],
  templateUrl: './wedding-info.component.html',
  styleUrl: './wedding-info.component.scss',
})
export class WeddingInfoComponent implements OnInit {
  private weddingInfoService = inject(WeddingInfoService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  loading = signal(true);
  saving = signal(false);
  isNew = signal(false);

  form = this.fb.nonNullable.group({
    brideName: ['', Validators.required],
    brideSurname: ['', Validators.required],
    groomName: ['', Validators.required],
    groomSurname: ['', Validators.required],
    approximateDate: ['', Validators.required],
    weddingName: [''],
    weddingDescription: [''],
    weddingDate: [''],
    // Civil
    civilLocationName: [''],
    civilLocationAddress: [''],
    civilLocationLatitude: [null as number | null],
    civilLocationLongitude: [null as number | null],
    civilLocationGoogleMapsUrl: [''],
    civilLocationAppleMapsUrl: [''],
    // Church
    churchLocationName: [''],
    churchLocationAddress: [''],
    churchLocationLatitude: [null as number | null],
    churchLocationLongitude: [null as number | null],
    churchLocationGoogleMapsUrl: [''],
    churchLocationAppleMapsUrl: [''],
    // Party
    partyLocationName: [''],
    partyLocationAddress: [''],
    partyLocationLatitude: [null as number | null],
    partyLocationLongitude: [null as number | null],
    partyLocationGoogleMapsUrl: [''],
    partyLocationAppleMapsUrl: [''],
    // House (admin only)
    houseLocationName: [''],
    houseLocationAddress: [''],
    houseLocationLatitude: [null as number | null],
    houseLocationLongitude: [null as number | null],
    houseLocationGoogleMapsUrl: [''],
    houseLocationAppleMapsUrl: [''],
  });

  ngOnInit(): void {
    this.weddingInfoService.get().subscribe({
      next: (info) => {
        this.patchForm(info);
        this.loading.set(false);
      },
      error: (err) => {
        if (err.status === 404) this.isNew.set(true);
        this.loading.set(false);
      },
    });
  }

  private patchForm(info: WeddingInfoDto): void {
    this.form.patchValue({
      brideName: info.brideName,
      brideSurname: info.brideSurname,
      groomName: info.groomName,
      groomSurname: info.groomSurname,
      approximateDate: info.approximateDate,
      weddingName: info.weddingName,
      weddingDescription: info.weddingDescription,
      weddingDate: info.weddingDate ? info.weddingDate.slice(0, 10) : '',
      civilLocationName: info.locationCivil?.name ?? '',
      civilLocationAddress: info.locationCivil?.address ?? '',
      civilLocationLatitude: info.locationCivil?.latitude ?? null,
      civilLocationLongitude: info.locationCivil?.longitude ?? null,
      civilLocationGoogleMapsUrl: info.locationCivil?.googleMapsUrl ?? '',
      civilLocationAppleMapsUrl: info.locationCivil?.appleMapsUrl ?? '',
      churchLocationName: info.locationChurch?.name ?? '',
      churchLocationAddress: info.locationChurch?.address ?? '',
      churchLocationLatitude: info.locationChurch?.latitude ?? null,
      churchLocationLongitude: info.locationChurch?.longitude ?? null,
      churchLocationGoogleMapsUrl: info.locationChurch?.googleMapsUrl ?? '',
      churchLocationAppleMapsUrl: info.locationChurch?.appleMapsUrl ?? '',
      partyLocationName: info.locationParty?.name ?? '',
      partyLocationAddress: info.locationParty?.address ?? '',
      partyLocationLatitude: info.locationParty?.latitude ?? null,
      partyLocationLongitude: info.locationParty?.longitude ?? null,
      partyLocationGoogleMapsUrl: info.locationParty?.googleMapsUrl ?? '',
      partyLocationAppleMapsUrl: info.locationParty?.appleMapsUrl ?? '',
      houseLocationName: info.locationHouse?.name ?? '',
      houseLocationAddress: info.locationHouse?.address ?? '',
      houseLocationLatitude: info.locationHouse?.latitude ?? null,
      houseLocationLongitude: info.locationHouse?.longitude ?? null,
      houseLocationGoogleMapsUrl: info.locationHouse?.googleMapsUrl ?? '',
      houseLocationAppleMapsUrl: info.locationHouse?.appleMapsUrl ?? '',
    });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const val = this.form.getRawValue();

    const dto: WeddingInfoUpdateDto = {
      brideName: val.brideName,
      brideSurname: val.brideSurname,
      groomName: val.groomName,
      groomSurname: val.groomSurname,
      approximateDate: val.approximateDate,
      weddingName: val.weddingName || undefined,
      weddingDescription: val.weddingDescription || undefined,
      weddingDate: val.weddingDate || undefined,
      civilLocationName: val.civilLocationName || undefined,
      civilLocationAddress: val.civilLocationAddress || undefined,
      civilLocationLatitude: val.civilLocationLatitude ?? undefined,
      civilLocationLongitude: val.civilLocationLongitude ?? undefined,
      civilLocationGoogleMapsUrl: val.civilLocationGoogleMapsUrl || undefined,
      civilLocationAppleMapsUrl: val.civilLocationAppleMapsUrl || undefined,
      churchLocationName: val.churchLocationName || undefined,
      churchLocationAddress: val.churchLocationAddress || undefined,
      churchLocationLatitude: val.churchLocationLatitude ?? undefined,
      churchLocationLongitude: val.churchLocationLongitude ?? undefined,
      churchLocationGoogleMapsUrl: val.churchLocationGoogleMapsUrl || undefined,
      churchLocationAppleMapsUrl: val.churchLocationAppleMapsUrl || undefined,
      partyLocationName: val.partyLocationName || undefined,
      partyLocationAddress: val.partyLocationAddress || undefined,
      partyLocationLatitude: val.partyLocationLatitude ?? undefined,
      partyLocationLongitude: val.partyLocationLongitude ?? undefined,
      partyLocationGoogleMapsUrl: val.partyLocationGoogleMapsUrl || undefined,
      partyLocationAppleMapsUrl: val.partyLocationAppleMapsUrl || undefined,
      houseLocationName: val.houseLocationName || undefined,
      houseLocationAddress: val.houseLocationAddress || undefined,
      houseLocationLatitude: val.houseLocationLatitude ?? undefined,
      houseLocationLongitude: val.houseLocationLongitude ?? undefined,
      houseLocationGoogleMapsUrl: val.houseLocationGoogleMapsUrl || undefined,
      houseLocationAppleMapsUrl: val.houseLocationAppleMapsUrl || undefined,
    };

    const req = this.isNew()
      ? this.weddingInfoService.initialize(dto)
      : this.weddingInfoService.update(dto);

    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.isNew.set(false);
        this.snackBar.open('Wedding info saved successfully', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.saving.set(false);
        this.snackBar.open(err.error?.error ?? 'Failed to save', 'Close', { duration: 4000 });
      },
    });
  }
}
