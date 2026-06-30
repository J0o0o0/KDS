import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { Order, OrderItemComponent } from '../models';

/**
 * !!! IMPORTANT — PLACEHOLDER EVENT NAMES !!!
 * Your KitchenHub.cs only defines client->server group methods
 * (JoinStationGroup, JoinExpediterGroup, etc). The actual SERVER -> CLIENT
 * event names are decided inside SignalRKitchenNotifier (IKitchenNotifier),
 * which wasn't shared. The three names below are my best-guess convention —
 * once you share SignalRKitchenNotifier.cs, just update the three string
 * literals in `connect()` to match exactly what it calls via
 * `Clients.Group(...).SendAsync("EventName", payload)`.
 */
const EVENTS = {
  ORDER_CREATED: 'OrderCreated',
  COMPONENT_STATUS_UPDATED: 'OrderComponentStatusUpdated',
  ORDER_STATUS_UPDATED: 'OrderStatusUpdated',
};

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection?: signalR.HubConnection;

  readonly connectionState = signal<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected
  );

  // Emits whenever a relevant push event arrives. Components subscribe to
  // these signals (or just re-fetch via REST on each emit, which is simplest
  // and safest given the small data volumes a KDS handles).
  readonly orderCreated = signal<Order | null>(null);
  readonly componentStatusUpdated = signal<{ orderId: number; component: OrderItemComponent } | null>(null);
  readonly orderStatusUpdated = signal<{ orderId: number; status: string } | null>(null);

  constructor(private auth: AuthService) {}

  connect(): void {
    if (this.hubConnection) return; // already connecting/connected

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        accessTokenFactory: () => this.auth.getToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on(EVENTS.ORDER_CREATED, (order: Order) => {
      this.orderCreated.set(order);
    });

    this.hubConnection.on(
      EVENTS.COMPONENT_STATUS_UPDATED,
      (payload: { orderId: number; component: OrderItemComponent }) => {
        this.componentStatusUpdated.set(payload);
      }
    );

    this.hubConnection.on(
      EVENTS.ORDER_STATUS_UPDATED,
      (payload: { orderId: number; status: string }) => {
        this.orderStatusUpdated.set(payload);
      }
    );

    this.hubConnection.onreconnecting(() => this.connectionState.set(signalR.HubConnectionState.Reconnecting));
    this.hubConnection.onreconnected(() => this.connectionState.set(signalR.HubConnectionState.Connected));
    this.hubConnection.onclose(() => this.connectionState.set(signalR.HubConnectionState.Disconnected));

    this.hubConnection
      .start()
      .then(() => this.connectionState.set(signalR.HubConnectionState.Connected))
      .catch((err) => console.error('SignalR connection failed:', err));
  }

  disconnect(): void {
    this.hubConnection?.stop();
    this.hubConnection = undefined;
    this.connectionState.set(signalR.HubConnectionState.Disconnected);
  }

  joinStationGroup(stationId: number): void {
    this.hubConnection?.invoke('JoinStationGroup', stationId).catch(console.error);
  }

  leaveStationGroup(stationId: number): void {
    this.hubConnection?.invoke('LeaveStationGroup', stationId).catch(console.error);
  }

  joinExpediterGroup(): void {
    this.hubConnection?.invoke('JoinExpediterGroup').catch(console.error);
  }

  leaveExpediterGroup(): void {
    this.hubConnection?.invoke('LeaveExpediterGroup').catch(console.error);
  }
}
