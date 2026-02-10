import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  CategoryDto, 
  CategoryTreeDto,
  CreateCategoryRequest, 
  UpdateCategoryRequest, 
  PagedResult, 
  QueryParameters 
} from '../models/category.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly basePath = '/api/category';

  constructor(private api: ApiService) {}

  getCategories(query: QueryParameters = {}): Observable<PagedResult<CategoryDto>> {
    let params = new HttpParams();
    
    if (query.pageNumber) params = params.set('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params = params.set('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
    if (query.includeDeleted !== undefined) params = params.set('includeDeleted', query.includeDeleted.toString());

    return this.api.get<PagedResult<CategoryDto>>(this.basePath, params);
  }

  getAllCategories(): Observable<CategoryDto[]> {
    return this.api.get<CategoryDto[]>(`${this.basePath}/all`);
  }

  getCategoryTree(): Observable<CategoryTreeDto[]> {
    return this.api.get<CategoryTreeDto[]>(`${this.basePath}/tree`);
  }

  getCategory(id: number): Observable<CategoryDto> {
    return this.api.get<CategoryDto>(`${this.basePath}/${id}`);
  }

  createCategory(request: CreateCategoryRequest): Observable<CategoryDto> {
    return this.api.post<CategoryDto>(this.basePath, request);
  }

  updateCategory(id: number, request: UpdateCategoryRequest): Observable<CategoryDto> {
    return this.api.put<CategoryDto>(`${this.basePath}/${id}`, request);
  }

  deleteCategory(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  hardDeleteCategory(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}/permanent`);
  }

  canAddChildCategory(parentId: number | null): Observable<{ canAdd: boolean }> {
    const path = parentId ? `${this.basePath}/can-add-child/${parentId}` : `${this.basePath}/can-add-child`;
    return this.api.get<{ canAdd: boolean }>(path);
  }
}
