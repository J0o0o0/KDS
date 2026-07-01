import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse, UserInfo } from './models';

/**
 * Auth service. Holds the JWT in a signal and mirrors it to localStorage
 * so the session survives page reloads.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly TOKEN_KEY = 'kds_token';
  private readonly USER_KEY = 'kds_user';

  private readonly _token = signal<string | null>(
    localStorage.getItem(this.TOKEN_KEY)
  );
  private readonly _user = signal<UserInfo | null>(this.parseStoredUser());

  readonly token = this._token.asReadonly();
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => !!this._token());
  readonly roles = computed(() => this._user()?.roles ?? []);

  hasRole(role: string): boolean {
    return this.roles().includes(role);
  }

  login(req: LoginRequest) {
    return this.http.post<LoginResponse>(
      `${environment.apiUrl}/auth/login`,
      req
    );
  }

  /** Call after a successful login to persist the session. */
  setSession(res: LoginResponse): void {
    const user: UserInfo = {
      email: res.email,
      roles: res.roles,
      fullName: res.fullName,
    };
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this._token.set(res.token);
    this._user.set(user);
  }

  /** Every role lands on /home — the HomeComponent picks the view. */
  navigateAfterLogin(): void {
    this.router.navigate(['/home']);
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this._token.set(null);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  private parseStoredUser(): UserInfo | null {
    const raw = localStorage.getItem(this.USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserInfo;
    } catch {
      return null;
    }
  }
}
