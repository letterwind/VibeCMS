import { RoleDto } from './role.model';

export interface UserDto {
  id: number;
  account: string;
  displayName: string;
  roles: RoleDto[];
  isPasswordExpired: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateUserRequest {
  account: string;
  password: string;
  displayName: string;
  roleIds: number[] | null;
}

export interface UpdateUserRequest {
  displayName: string;
  newPassword: string | null;
  roleIds: number[] | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface QueryParameters {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  searchTerm?: string;
  includeDeleted?: boolean;
}

export interface ValidationResponse {
  isValid: boolean;
  errors: string[];
}
