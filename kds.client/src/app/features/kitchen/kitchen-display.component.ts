import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { OrdersService } from '../../core/services/orders.service';
import { StationsService } from '../../core/services/stations.service';
import { SignalrService } from '../../core/services/signalr.service';
import { TopBarComponent } from '../../shared/top-bar.component';
import { Order, OrderItemComponent, OrderStatus, Station } from '../../core/models';

interface Ticket {
  key: string;
  order: Order;
  component: OrderItemComponent & { stationColor?: string | null };
  itemNotes?: string | null;
}

/** All tickets belonging to one order, clustered together for display. */
interface OrderCluster {
  order: Order;
  agePercent: number;
  tickets: Ticket[];
}

@Component({
  selector: 'app-kitchen-display',
  standalone: true,
  imports: [CommonModule, TopBarComponent],
  templateUrl: './kitchen-display.component.html',
  styleUrls: ['./kitchen-display.component.scss'],
})
export class KitchenDisplayComponent implements OnInit, OnDestroy {
  readonly orders = signal<Order[]>([]);
  readonly stations = signal<Station[]>([]);

  // Only true on the very first load — prevents the whole grid from
  // blanking out and re-painting (flicker) on every background poll.
  readonly loading = signal(true);
  private hasLoadedOnce = false;

  // Station filter tab, Expediter/Admin only. null = "All stations".
  readonly stationFilter = signal<number | null>(null);

  // Fallback-only state: used until backend returns a real assigned station.
  private readonly fallbackStationId = signal<number | null>(null);

  private pollHandle?: ReturnType<typeof setInterval>;

  constructor(
    private auth: AuthService,
    private ordersSvc: OrdersService,
    private stationsSvc: StationsService,
    private signalr: SignalrService
  ) { }

  isCook = computed(() => this.auth.hasRole('Cook') && !this.auth.hasRole('Expediter', 'Admin'));
  isExpediter = computed(() => this.auth.hasRole('Expediter', 'Admin'));

  assignedStationId = computed<number | null>(
    () => this.auth.currentUser()?.stationId ?? this.fallbackStationId() ?? null
  );

  stationName = computed<string | null>(() => {
    const id = this.assignedStationId();
    return this.stations().find((s) => s.id === id)?.name ?? null;
  });

  /** Active stations available to filter by (Expediter/Admin tab bar). */
  activeStations = computed(() => this.stations().filter((s) => s.isActive));

  ngOnInit(): void {
    this.stationsSvc.getAll().subscribe((s) => this.stations.set(s));
    this.refresh();
    this.startPolling();
    this.signalr.connect();

    if (this.isExpediter()) {
      this.signalr.joinExpediterGroup();
    }
  }

  ngOnDestroy(): void {
    if (this.pollHandle) clearInterval(this.pollHandle);
    if (this.isExpediter()) this.signalr.leaveExpediterGroup();
    const stationId = this.assignedStationId();
    if (stationId) this.signalr.leaveStationGroup(stationId);
  }

  selectFallbackStation(station: Station): void {
    this.fallbackStationId.set(station.id);
    this.signalr.joinStationGroup(station.id);
    this.refresh();
  }

  setStationFilter(stationId: number | null): void {
    this.stationFilter.set(stationId);
  }

  /** Orders grouped into clusters, each containing only the tickets this screen should show. */
  orderClusters = computed<OrderCluster[]>(() => {
    const now = Date.now();
    const filterStationId = this.isExpediter() ? this.stationFilter() : this.assignedStationId();

    const clusters: OrderCluster[] = [];

    for (const order of this.orders()) {
      if (order.status === 'Served' || order.status === 'Cancelled') continue;

      const tickets: Ticket[] = [];

      for (const item of order.items) {
        for (const comp of item.components) {
          if (comp.status === 'Served' || comp.status === 'Bumped') continue;

          if (filterStationId !== null && comp.stationId !== filterStationId) continue;

          const station = this.stations().find((s) => s.id === comp.stationId);
          tickets.push({
            key: `${order.id}-${comp.id}`,
            order,
            component: { ...comp, stationColor: station?.color },
            itemNotes: item.notes,
          });
        }
      }

      if (tickets.length === 0) continue;

      const ageMs = now - new Date(order.createdAt).getTime();
      const agePercent = Math.min(100, (ageMs / (15 * 60 * 1000)) * 100); // 15 min = full bar

      clusters.push({ order, agePercent, tickets });
    }

    // Oldest order first — FIFO ticket rail behavior.
    return clusters.sort(
      (a, b) => new Date(a.order.createdAt).getTime() - new Date(b.order.createdAt).getTime()
    );
  });

  advanceComponent(ticket: Ticket, next: OrderStatus): void {
    this.ordersSvc.updateComponentStatus(ticket.order.id, ticket.component.id, next).subscribe(() => {
      this.refresh();
    });
  }

  bumpOrder(order: Order): void {
    this.ordersSvc.updateOrderStatus(order.id, 'Served').subscribe(() => this.refresh());
  }

  private refresh(): void {
    if (!this.hasLoadedOnce) this.loading.set(true);

    const stationId = this.assignedStationId();

    const obs = this.isExpediter()
      ? this.ordersSvc.getActive()
      : stationId
        ? this.ordersSvc.getByStation(stationId)
        : null;

    if (!obs) {
      this.loading.set(false);
      this.hasLoadedOnce = true;
      return;
    }

    obs.subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
        this.hasLoadedOnce = true;
      },
      error: () => {
        this.loading.set(false);
        this.hasLoadedOnce = true;
      },
    });
  }

  private startPolling(): void {
    // Polling as a safety net alongside SignalR push, in case events are
    // missed or SignalRKitchenNotifier event names don't match the
    // placeholders yet. Safe to remove once real-time push is confirmed working.
    this.pollHandle = setInterval(() => this.refresh(), 15000);
  }
}
