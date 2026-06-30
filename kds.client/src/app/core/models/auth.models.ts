export type Role = 'Admin' | 'Cashier' | 'Cook' | 'Expediter';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  roles: Role[];
  stationId?: number | null; // only applied server-side if roles includes 'Cook'
}

export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
  roles: Role[];
  expiresAt: string; // ISO date string
  stationId?: number | null;
  stationName?: string | null;
}

export interface CurrentUser {
  email: string;
  fullName: string;
  roles: Role[];
  isActive: boolean;
  stationId?: number | null;
  stationName?: string | null;
}

// Matches backend UserDto — used by the Admin "manage users" list.
export interface ManagedUser {
  id: string;
  email: string;
  fullName: string;
  isActive: boolean;
  createdAt: string; // ISO date string
  roles: Role[];
  stationId?: number | null;
  stationName?: string | null;
}
