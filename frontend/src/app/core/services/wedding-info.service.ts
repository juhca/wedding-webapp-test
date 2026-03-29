import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WeddingInfoDto, WeddingInfoUpdateDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class WeddingInfoService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/WeddingInfo`;

  get(): Observable<WeddingInfoDto> {
    return this.http.get<WeddingInfoDto>(this.base);
  }

  initialize(dto: WeddingInfoUpdateDto): Observable<WeddingInfoDto> {
    return this.http.post<WeddingInfoDto>(`${this.base}/initialize`, dto);
  }

  update(dto: WeddingInfoUpdateDto): Observable<WeddingInfoDto> {
    return this.http.put<WeddingInfoDto>(this.base, dto);
  }
}
