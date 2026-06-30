import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AddOn, CreateAddOnRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class AddOnsService {
  private readonly base = `${environment.apiUrl}/addons`;

  constructor(private http: HttpClient) {}

  getAll(activeOnly = false): Observable<AddOn[]> {
    return this.http.get<AddOn[]>(this.base, { params: { activeOnly } });
  }

  create(dto: CreateAddOnRequest): Observable<AddOn> {
    return this.http.post<AddOn>(this.base, dto);
  }

  toggleActive(id: number): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/toggle-active`, {});
  }
}
