import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {
  LoginRequest,
  LoginResponse,
  CaptchaResponse,
  ChangePasswordRequest,
  UserDto
} from '../models/auth.model';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly basePath = '/auth';
  private readonly TOKEN_KEY = 'token';
  private readonly REFRESH_TOKEN_KEY = 'refreshToken';
  private readonly USER_KEY = 'user';

  private currentUserSignal = signal<UserDto | null>(this.loadUserFromStorage());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.currentUserSignal());

  constructor(private api: ApiService) {}

  getCaptcha(): Observable<CaptchaResponse> {
    return this.api.get<CaptchaResponse>(`${this.basePath}/captcha`);
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse>(`${this.basePath}/login`, request).pipe(
      tap(response => {
        this.setToken(response.token);
        this.setRefreshToken(response.refreshToken);
        this.setUser(response.user);
      })
    );
  }

  logout(): Observable<void> {
    return this.api.post<void>(`${this.basePath}/logout`, {}).pipe(
      tap(() => this.clearAuth())
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.api.post<void>(`${this.basePath}/change-password`, request);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  clearAuth(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSignal.set(null);
  }

  private setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  private setRefreshToken(refreshToken: string): void {
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  private setUser(user: UserDto): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSignal.set(user);
  }

  private loadUserFromStorage(): UserDto | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch {
        return null;
      }
    }
    return null;
  }
}
