// ---------- Stations ----------
export interface Station {
  id: number;
  name: string;
  color?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface CreateStationRequest {
  name: string;
  color?: string | null;
  sortOrder: number;
  isActive: boolean;
}

// ---------- Add-ons ----------
export interface AddOn {
  id: number;
  name: string;
  price: number;
  isRemoval: boolean;
  isActive: boolean;
}

export interface CreateAddOnRequest {
  name: string;
  price: number;
  isRemoval: boolean;
  isActive: boolean;
}

// ---------- Components (build-your-own-meal pieces) ----------
export interface ComponentVariant {
  id: number;
  name: string;
  priceDelta: number;
  isDefault: boolean;
  isActive: boolean;
}

export interface AllowedAddOn {
  addOnId: number;
  addOnName: string;
  maxQuantity: number;
}

export interface SwapOption {
  componentId: number;
  componentName: string;
}

export interface MenuComponent {
  id: number;
  name: string;
  description?: string | null;
  defaultStationId: number;
  defaultStationName?: string | null;
  isActive: boolean;
  variants: ComponentVariant[];
  allowedAddOns: AllowedAddOn[];
  swappableWith: SwapOption[];
}

export interface ComponentAddOnLink {
  addOnId: number;
  maxQuantity: number;
}

export interface CreateVariantRequest {
  name: string;
  priceDelta: number;
  isDefault: boolean;
}

export interface CreateComponentRequest {
  name: string;
  description?: string | null;
  defaultStationId: number;
  defaultStationName: string;
  isActive: boolean;
  addOns: ComponentAddOnLink[];
  swappableWithIds: number[];
  variants: CreateVariantRequest[];
}
