import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User, CreateUserRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/Users`;

  getAll(): Observable<User[]> {
    return this.http.get<User[]>(`${this.base}/GetAll`);
  }

  addUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(`${this.base}/AddUser`, request);
  }
}
