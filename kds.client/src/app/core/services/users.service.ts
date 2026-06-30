import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ManagedUser, Role } from '../models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly base = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ManagedUser[]> {
    return this.http.get<ManagedUser[]>(this.base);
  }

  toggleActive(id: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/toggle-active`, {});
  }

  addRole(id: string, role: Role): Observable<ManagedUser> {
    return this.http.post<ManagedUser>(`${this.base}/${id}/roles`, { role });
  }

  removeRole(id: string, role: Role): Observable<ManagedUser> {
    return this.http.delete<ManagedUser>(`${this.base}/${id}/roles/${role}`);
  }

  assignStation(id: string, stationId: number | null): Observable<ManagedUser> {
    return this.http.patch<ManagedUser>(`${this.base}/${id}/station`, { stationId });
  }
}
