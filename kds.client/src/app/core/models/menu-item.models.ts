export interface MenuItemComponent {
  componentId: number;
  componentName: string;
  quantity: number;
}

export interface MenuItem {
  id: number;
  name: string;
  description?: string | null;
  basePrice: number;
  category: string;
  prepTimeMinutes: number;
  isActive: boolean;
  components: MenuItemComponent[];
}

export interface CreateMenuItemComponentRequest {
  componentId: number;
  quantity: number;
}

export interface CreateMenuItemRequest {
  name: string;
  description?: string | null;
  basePrice: number;
  category: string;
  prepTimeMinutes: number;
  isActive: boolean;
  components: CreateMenuItemComponentRequest[];
}

// NOTE: backend's UpdateMenuItemDto also includes stationId/price (singular)
// fields that don't match CreateMenuItemDto 1:1. Flagged for you to reconcile
// server-side — frontend follows CreateMenuItemDto shape since that's what
// MenuItemsController currently uses for both create scenarios.
