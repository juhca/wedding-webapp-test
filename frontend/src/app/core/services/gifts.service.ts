import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GiftDto, CreateGiftDto, UpdateGiftDto, ImportGiftsResult, ReserveGiftDto, GiftReservationConfirmation } from '../models/models';

@Injectable({ providedIn: 'root' })
export class GiftsService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/Gifts`;

  getAll(): Observable<GiftDto[]> {
    return this.http.get<GiftDto[]>(this.base);
  }

  getById(id: string): Observable<GiftDto> {
    return this.http.get<GiftDto>(`${this.base}/${id}`);
  }

  // Guest endpoints
  getMyReservations(): Observable<GiftDto[]> {
    return this.http.get<GiftDto[]>(`${this.base}/my-reservations`);
  }

  reserve(id: string, dto: ReserveGiftDto): Observable<GiftReservationConfirmation> {
    return this.http.post<GiftReservationConfirmation>(`${this.base}/${id}/reserve`, dto);
  }

  unreserve(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}/reserve`);
  }

  // Admin endpoints
  create(dto: CreateGiftDto): Observable<GiftDto> {
    return this.http.post<GiftDto>(this.base, dto);
  }

  update(id: string, dto: UpdateGiftDto): Observable<GiftDto> {
    return this.http.put<GiftDto>(`${this.base}/${id}`, dto);
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  importJson(dtos: CreateGiftDto[]): Observable<ImportGiftsResult> {
    return this.http.post<ImportGiftsResult>(`${this.base}/import`, dtos);
  }

  importCsv(file: File): Observable<ImportGiftsResult> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ImportGiftsResult>(`${this.base}/import/csv`, formData);
  }
}
