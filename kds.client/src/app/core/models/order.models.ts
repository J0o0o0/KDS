export type OrderType = 'DineIn' | 'Pickup';

// Matches UpdateStatusDto.Status values used across the kitchen flow
export type OrderStatus =
  | 'New'
  | 'InProgress'
  | 'Ready'
  | 'Bumped'
  | 'Served'
  | 'Cancelled';

// ---------- Read models (server -> client) ----------
export interface OrderItemComponentAddOn {
  addOnName: string;
  quantity: number;
  isRemoval: boolean;
}

export interface OrderItemComponent {
  id: number;
  componentName: string;
  variantName: string;
  quantity: number;
  stationName: string;
  stationId: number;
  status: OrderStatus;
  addOns: OrderItemComponentAddOn[];
}

export interface OrderItem {
  id: number;
  menuItemName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  notes?: string | null;
  status: OrderStatus;
  components: OrderItemComponent[];
}

export interface Order {
  id: number;
  orderNumber: number;
  orderType: OrderType;
  tableNumber?: string | null;
  customerName?: string | null;
  status: OrderStatus;
  totalAmount: number;
  notes?: string | null;
  createdAt: string; // ISO date string
  readyAt?: string | null;
  servedAt?: string | null;
  cashierName: string;
  items: OrderItem[];
}

// ---------- Write models (client -> server) ----------
export interface CreateAddOnSelectionRequest {
  addOnId: number;
  quantity: number;
}

export interface CreateComponentConfigRequest {
  componentId: number;
  variantId?: number | null;
  quantity: number;
  addOns: CreateAddOnSelectionRequest[];
}

export interface CreateOrderItemRequest {
  menuItemId: number;
  quantity: number;
  notes?: string | null;
  components: CreateComponentConfigRequest[];
}

export interface CreateOrderRequest {
  orderType: OrderType;
  tableNumber?: string | null;
  customerName?: string | null;
  notes?: string | null;
  items: CreateOrderItemRequest[];
}

export interface UpdateStatusRequest {
  status: OrderStatus;
}
