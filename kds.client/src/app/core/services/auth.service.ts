import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, CurrentUser, LoginRequest, RegisterRequest, Role } from '../models';

const TOKEN_KEY = 'kds_token';
const USER_KEY = 'kds_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Signal-based current user state. Null = not logged in.
  private readonly _currentUser = signal<CurrentUser | null>(this.loadStoredUser());
  readonly currentUser = this._currentUser.asReadonly();

  readonly isLoggedIn = computed(() => this._currentUser() !== null);
  readonly roles = computed<Role[]>(() => this._currentUser()?.roles ?? []);

  constructor(private http: HttpClient, private router: Router) {}

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, payload).pipe(
      tap((res) => this.persistSession(res))
    );
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    // Admin-only endpoint — call from the Admin "create user" screen.
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, payload);
  }

  refreshMe(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${environment.apiUrl}/auth/me`).pipe(
      tap((user) => {
        this._currentUser.set(user);
        localStorage.setItem(USER_KEY, JSON.stringify(user));
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  hasRole(...allowed: Role[]): boolean {
    const userRoles = this.roles();
    return allowed.some((r) => userRoles.includes(r));
  }

  /** Where to send the user right after login, based on their first matching role. */
  homeRouteForCurrentUser(): string {
    const r = this.roles();
    if (r.includes('Admin')) return '/admin';
    if (r.includes('Cashier')) return '/cashier';
    if (r.includes('Cook') || r.includes('Expediter')) return '/kitchen';
    return '/login';
  }

  private persistSession(res: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, res.token);
    const user: CurrentUser = {
      email: res.email,
      fullName: res.fullName,
      roles: res.roles,
      isActive: true,
      stationId: res.stationId,
      stationName: res.stationName,
    };
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this._currentUser.set(user);
  }

  private loadStoredUser(): CurrentUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }
}
