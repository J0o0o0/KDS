import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrdersService } from '../../core/services/orders.service';
import { TopBarComponent } from '../../shared/top-bar.component';
import { Order, OrderStatus } from '../../core/models';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, FormsModule, TopBarComponent],
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.scss'],
})
export class OrderHistoryComponent implements OnInit {
  readonly statuses: OrderStatus[] = ['New', 'InProgress', 'Ready', 'Bumped', 'Served', 'Cancelled'];

  readonly orders = signal<Order[]>([]);
  readonly loading = signal(true);
  readonly expandedId = signal<number | null>(null);

  fromDate = '';
  toDate = '';
  statusFilter: OrderStatus | null = null;

  constructor(private ordersSvc: OrdersService) {}

  ngOnInit(): void {
    this.search();
  }

  search(): void {
    this.loading.set(true);
    this.ordersSvc
      .getAll({
        from: this.fromDate || null,
        to: this.toDate || null,
        status: this.statusFilter,
      })
      .subscribe({
        next: (orders) => {
          this.orders.set(orders);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  toggleExpand(orderId: number): void {
    this.expandedId.set(this.expandedId() === orderId ? null : orderId);
  }

  statusClass(status: OrderStatus): string {
    switch (status) {
      case 'Served':
        return 'bg-emerald-100 text-emerald-700';
      case 'Cancelled':
        return 'bg-red-100 text-red-700';
      case 'Ready':
        return 'bg-blue-100 text-blue-700';
      case 'InProgress':
        return 'bg-amber-100 text-amber-700';
      default:
        return 'bg-slate-100 text-slate-600';
    }
  }
}
