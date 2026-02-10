import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  ArticleDto, 
  CreateArticleRequest, 
  UpdateArticleRequest, 
  PagedResult, 
  QueryParameters 
} from '../models/article.model';

@Injectable({
  providedIn: 'root'
})
export class ArticleService {
  private readonly basePath = '/api/article';

  constructor(private api: ApiService) {}

  getArticles(query: QueryParameters = {}, categoryId?: number): Observable<PagedResult<ArticleDto>> {
    let params = new HttpParams();
    
    if (query.pageNumber) params = params.set('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params = params.set('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
    if (query.includeDeleted !== undefined) params = params.set('includeDeleted', query.includeDeleted.toString());
    if (categoryId !== undefined) params = params.set('categoryId', categoryId.toString());

    return this.api.get<PagedResult<ArticleDto>>(this.basePath, params);
  }

  getArticle(id: number): Observable<ArticleDto> {
    return this.api.get<ArticleDto>(`${this.basePath}/${id}`);
  }

  createArticle(request: CreateArticleRequest): Observable<ArticleDto> {
    return this.api.post<ArticleDto>(this.basePath, request);
  }

  updateArticle(id: number, request: UpdateArticleRequest): Observable<ArticleDto> {
    return this.api.put<ArticleDto>(`${this.basePath}/${id}`, request);
  }

  deleteArticle(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  hardDeleteArticle(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}/permanent`);
  }

  getAllTags(): Observable<string[]> {
    return this.api.get<string[]>(`${this.basePath}/tags`);
  }
}
