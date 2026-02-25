export interface ArticleDto
{
  id: string | number;
  title: string;
  content: string;
  slug: string;
  categoryId: string | number;
  categoryName?: string | null;
  tags?: string[];
  summary?: string;
  metaTitle?: string | null;
  metaDescription?: string | null;
  metaKeywords?: string | null;
  published?: boolean;
  languageCode: string;
  availableLanguages: Record<string, boolean>;
  createdAt: Date;
  updatedAt: Date;
  createdBy?: number | null;
  createdByName?: string | null;
}

export interface CreateArticleRequest
{
  title: string;
  content: string;
  slug: string;
  categoryId: number;
  tags: string[] | null;
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
}

export interface UpdateArticleRequest
{
  title: string;
  content: string;
  slug: string;
  categoryId: number;
  tags: string[] | null;
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
}

export interface PagedResult<T>
{
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface QueryParameters
{
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  searchTerm?: string;
  includeDeleted?: boolean;
}
