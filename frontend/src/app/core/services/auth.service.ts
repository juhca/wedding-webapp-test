import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginResponseDto, RefreshTokenRequest, UserRole } from '../models/models';

const TOKEN_KEY = 'wedding_token';
const REFRESH_TOKEN_KEY = 'wedding_refresh_token';
const USER_KEY = 'wedding_user';

export interface SessionUser {
  userId: string;
  email: string;
  role: UserRole;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = environment.apiUrl;

  private _user = signal<SessionUser | null>(this.loadUser());

  readonly currentUser = this._user.asReadonly();
  readonly isLoggedIn = computed(() => !!this._user());

  adminLogin(email: string, password: string): Observable<LoginResponseDto> {
    return this.http
      .post<LoginResponseDto>(`${this.apiUrl}/api/Auth/AdminLogin`, { email, password })
      .pipe(tap((res) => this.storeSession(res)));
  }

  guestLogin(accessCode: string): Observable<LoginResponseDto> {
    return this.http
      .post<LoginResponseDto>(`${this.apiUrl}/api/Auth/GuestLogin`, { accessCode })
      .pipe(tap((res) => this.storeSession(res)));
  }

  refresh(): Observable<LoginResponseDto> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY) ?? '';
    return this.http
      .post<LoginResponseDto>(`${this.apiUrl}/api/Auth/Refresh`, { refreshToken } as RefreshTokenRequest)
      .pipe(tap((res) => this.storeSession(res)));
  }

  revoke(): Observable<unknown> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY) ?? '';
    return this.http.post(`${this.apiUrl}/api/Auth/Revoke`, { refreshToken } as RefreshTokenRequest);
  }

  logout(): void {
    this.revoke().subscribe({ error: () => {} });
    this.clearSession();
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private storeSession(res: LoginResponseDto): void {
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken.token);
    const user = this.decodeToken(res.token);
    if (user) {
      localStorage.setItem(USER_KEY, JSON.stringify(user));
      this._user.set(user);
    }
  }

  private clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
  }

  private loadUser(): SessionUser | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  /** Decode JWT payload without a library */
  private decodeToken(token: string): SessionUser | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // ASP.NET Core uses long claim type URIs
      const email =
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ??
        payload['email'] ?? '';
      const roleRaw =
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
        payload['role'] ?? '';
      const userId =
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
        payload['sub'] ?? '';
      const role: UserRole = UserRole[roleRaw as keyof typeof UserRole] ?? UserRole.LimitedExperience;
      return { userId, email, role };
    } catch {
      return null;
    }
  }
}
