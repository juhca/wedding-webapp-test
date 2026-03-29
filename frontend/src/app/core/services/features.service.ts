import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ModulesDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class FeaturesService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/Features`;

  getModules(): Observable<ModulesDto> {
    return this.http.get<ModulesDto>(this.base);
  }
}
