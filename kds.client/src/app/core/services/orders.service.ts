import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateOrderRequest, Order, OrderStatus } from '../models';

export interface OrderHistoryFilters {
  from?: string | null; // ISO date string, e.g. '2026-06-01'
  to?: string | null;
  status?: OrderStatus | null;
}

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly base = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  create(dto: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.base, dto);
  }

  getById(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.base}/${id}`);
  }

  getActive(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.base}/active`);
  }

  getByStation(stationId: number): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.base}/station/${stationId}`);
  }

  /** Order history — all orders (including served/cancelled), optional filters. */
  getAll(filters: OrderHistoryFilters = {}): Observable<Order[]> {
    const params: Record<string, string> = {};
    if (filters.from) params['from'] = filters.from;
    if (filters.to) params['to'] = filters.to;
    if (filters.status) params['status'] = filters.status;
    return this.http.get<Order[]>(this.base, { params });
  }

  updateComponentStatus(orderId: number, componentId: number, status: OrderStatus): Observable<void> {
    return this.http.patch<void>(`${this.base}/${orderId}/components/${componentId}/status`, { status });
  }

  updateOrderStatus(orderId: number, status: OrderStatus): Observable<void> {
    return this.http.patch<void>(`${this.base}/${orderId}/status`, { status });
  }
}
