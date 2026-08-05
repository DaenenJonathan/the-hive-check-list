export interface User {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export enum UserRole {
  Admin = 'Admin',
  Manager = 'Manager',
  WarehouseUser = 'WarehouseUser',
  Viewer = 'Viewer'
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAt: string;
}
