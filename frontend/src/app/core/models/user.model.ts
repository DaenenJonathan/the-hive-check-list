export interface User {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  agencyId?: string | null;
}

export enum UserRole {
  Admin = 'Admin',
  Manager = 'Manager',
  WarehouseUser = 'WarehouseUser',
  Viewer = 'Viewer',
  AgencyManager = 'AgencyManager'
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RequestAccountRequest {
  firstName: string;
  lastName: string;
  email: string;
  message?: string | null;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAt: string;
}
