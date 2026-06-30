import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateStationRequest, Station } from '../models';

@Injectable({ providedIn: 'root' })
export class StationsService {
  private readonly base = `${environment.apiUrl}/stations`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Station[]> {
    return this.http.get<Station[]>(this.base);
  }

  getOne(id: number): Observable<Station> {
    return this.http.get<Station>(`${this.base}/${id}`);
  }

  create(dto: CreateStationRequest): Observable<Station> {
    return this.http.post<Station>(this.base, dto);
  }

  update(id: number, dto: CreateStationRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, dto);
  }

  toggleActive(id: number): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/toggle-active`, {});
  }
}
