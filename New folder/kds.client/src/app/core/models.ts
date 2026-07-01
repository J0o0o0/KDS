/** App roles — must match the backend role names. */
export const ROLES = {
  ADMIN: 'Admin',
  CASHIER: 'Cashier',
  COOK: 'Cook',
  EXPEDITER: 'Expediter',
} as const;

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  roles: string[];
  fullName: string;
}

export interface UserInfo {
  email: string;
  roles: string[];
  fullName: string;
}
