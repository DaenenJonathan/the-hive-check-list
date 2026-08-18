import { UserRole } from '../../../core/models/user.model';

export interface UserBrandDto {
  id: string;
  name: string;
}

export interface UserAdminDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  agencyId: string | null;
  agencyName: string | null;
  brands: UserBrandDto[];
}

export interface UpdateUserRoleRequest {
  role: UserRole;
  agencyId: string | null;
  brandIds: string[];
}

export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  agencyId: string | null;
  brandIds: string[];
}

export interface CreateUserResult {
  userId: string;
  emailSent: boolean;
  temporaryPassword: string | null;
}
