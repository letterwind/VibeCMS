export interface RoleDto {
  id: number;
  name: string;
  description: string | null;
  hierarchyLevel: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateRoleRequest {
  name: string;
  description: string | null;
  hierarchyLevel: number;
}

export interface UpdateRoleRequest {
  name: string;
  description: string | null;
  hierarchyLevel: number;
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
