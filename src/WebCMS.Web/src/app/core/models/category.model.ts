export interface CategoryDto {
  id: number;
  name: string;
  slug: string;
  parentId: number | null;
  level: number;
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
  sortOrder: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CategoryTreeDto {
  id: number;
  name: string;
  slug: string;
  parentId: number | null;
  level: number;
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
  sortOrder: number;
  createdAt: Date;
  updatedAt: Date;
  children: CategoryTreeDto[] | null;
}

export interface CreateCategoryRequest {
  name: string;
  slug: string;
  parentId: number | null;
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
  sortOrder: number;
}

export interface UpdateCategoryRequest {
  name: string;
  slug: string;
  parentId: number | null;
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
  sortOrder: number;
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
