import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import
{
  ArticleDto,
  CreateArticleRequest,
  UpdateArticleRequest,
  PagedResult,
  QueryParameters
} from '../models/article.model';

@Injectable({
  providedIn: 'root'
})
export class ArticleService
{
  private readonly basePath = '/article';
  private readonly basePathForTranslation = '/articles';

  constructor(private api: ApiService) { }

  getArticles(query: QueryParameters = {}, categoryId?: number): Observable<PagedResult<ArticleDto>>
  {
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

  getArticle(id: number): Observable<ArticleDto>
  {
    return this.api.get<ArticleDto>(`${this.basePath}/${id}`);
  }

  createArticle(request: CreateArticleRequest): Observable<ArticleDto>
  {
    return this.api.post<ArticleDto>(this.basePath, request);
  }

  updateArticle(id: number, request: UpdateArticleRequest): Observable<ArticleDto>
  {
    return this.api.put<ArticleDto>(`${this.basePath}/${id}`, request);
  }

  deleteArticle(id: number): Observable<void>
  {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  hardDeleteArticle(id: number): Observable<void>
  {
    return this.api.delete<void>(`${this.basePath}/${id}/permanent`);
  }

  getAllTags(): Observable<string[]>
  {
    return this.api.get<string[]>(`${this.basePath}/tags`);
  }

  // ============ 多語言支援方法 ============

  /**
   * 獲取文章的所有語言版本
   */
  getArticleTranslations(id: number | string): Observable<ArticleDto[]>
  {
    return this.api.get<ArticleDto[]>(`${this.basePathForTranslation}/${id}/translations`);
  }

  /**
   * 獲取文章的翻譯狀態
   */
  getArticleTranslationStatus(id: number | string): Observable<Record<string, boolean>>
  {
    return this.api.get<Record<string, boolean>>(`${this.basePathForTranslation}/${id}/translations/status`);
  }

  /**
   * 複製文章翻譯
   * @param id 文章 ID
   * @param sourceLanguage 源語言代碼
   * @param targetLanguage 目標語言代碼
   */
  copyArticleTranslation(
    id: number | string,
    sourceLanguage: string,
    targetLanguage: string
  ): Observable<ArticleDto>
  {
    // The backend expects sourceLanguage and targetLanguage as query parameters.
    const qs = `?sourceLanguage=${encodeURIComponent(sourceLanguage)}&targetLanguage=${encodeURIComponent(targetLanguage)}`;
    // POST with empty body (server reads from query)
    return this.api.post<ArticleDto>(`${this.basePathForTranslation}/${id}/translations/copy${qs}`, {});
  }

  /**
   * 刪除特定語言的文章版本
   * @param id 文章 ID
   * @param languageCode 語言代碼
   */
  deleteLanguageVersion(id: number | string, languageCode: string): Observable<void>
  {
    return this.api.delete<void>(`${this.basePathForTranslation}/${id}/translations/${languageCode}`);
  }
}
