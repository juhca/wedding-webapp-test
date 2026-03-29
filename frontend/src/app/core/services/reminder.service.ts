import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReminderDto, AddReminderDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ReminderService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/reminders`;

  getRsvpReminders(): Observable<ReminderDto[]> {
    return this.http.get<ReminderDto[]>(`${this.base}/rsvp`);
  }

  addRsvpReminder(dto: AddReminderDto): Observable<ReminderDto> {
    return this.http.post<ReminderDto>(`${this.base}/rsvp`, dto);
  }

  getGiftReminders(giftId: string): Observable<ReminderDto[]> {
    return this.http.get<ReminderDto[]>(`${this.base}/gifts/${giftId}`);
  }

  addGiftReminder(giftId: string, dto: AddReminderDto): Observable<ReminderDto> {
    return this.http.post<ReminderDto>(`${this.base}/gifts/${giftId}`, dto);
  }

  deleteReminder(reminderId: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${reminderId}`);
  }
}
