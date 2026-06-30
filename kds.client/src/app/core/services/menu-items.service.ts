import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateMenuItemRequest, MenuItem } from '../models';

@Injectable({ providedIn: 'root' })
export class MenuItemsService {
  private readonly base = `${environment.apiUrl}/menuitems`;

  constructor(private http: HttpClient) {}

  getAll(activeOnly = false): Observable<MenuItem[]> {
    return this.http.get<MenuItem[]>(this.base, { params: { activeOnly } });
  }

  create(dto: CreateMenuItemRequest): Observable<MenuItem> {
    return this.http.post<MenuItem>(this.base, dto);
  }

  update(id: number, dto: CreateMenuItemRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, dto);
  }

  toggleActive(id: number): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/toggle-active`, {});
  }
}
