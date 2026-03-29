import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RsvpSummary, RsvpWithUser, RsvpDto, CreateRsvpDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class RsvpService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/Rsvp`;

  // Guest endpoints
  getMyRsvp(): Observable<RsvpDto> {
    return this.http.get<RsvpDto>(`${this.base}/my`);
  }

  createOrUpdate(dto: CreateRsvpDto): Observable<RsvpDto> {
    return this.http.post<RsvpDto>(this.base, dto);
  }

  // Admin endpoints
  getSummary(): Observable<RsvpSummary> {
    return this.http.get<RsvpSummary>(`${this.base}/summary`);
  }

  getAll(): Observable<RsvpWithUser[]> {
    return this.http.get<RsvpWithUser[]>(`${this.base}/all`);
  }

  exportCatering(): Observable<Blob> {
    return this.http.get(`${this.base}/export/catering`, { responseType: 'blob' });
  }
}
