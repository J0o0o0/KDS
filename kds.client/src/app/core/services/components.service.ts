import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AllowedAddOn,
  CreateComponentRequest,
  CreateVariantRequest,
  MenuComponent,
} from '../models';

@Injectable({ providedIn: 'root' })
export class ComponentsService {
  private readonly base = `${environment.apiUrl}/components`;

  constructor(private http: HttpClient) {}

  getAll(activeOnly = false): Observable<MenuComponent[]> {
    return this.http.get<MenuComponent[]>(this.base, { params: { activeOnly } });
  }

  create(dto: CreateComponentRequest): Observable<MenuComponent> {
    return this.http.post<MenuComponent>(this.base, dto);
  }

  update(id: number, dto: CreateComponentRequest): Observable<MenuComponent> {
    return this.http.put<MenuComponent>(`${this.base}/${id}`, dto);
  }

  addVariant(componentId: number, dto: CreateVariantRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/${componentId}/variants`, dto);
  }

  addAllowedAddOn(componentId: number, dto: AllowedAddOn): Observable<void> {
    return this.http.post<void>(`${this.base}/${componentId}/addons`, dto);
  }

  addSwap(componentIdA: number, componentIdB: number): Observable<void> {
    return this.http.post<void>(`${this.base}/${componentIdA}/swaps/${componentIdB}`, {});
  }
}
