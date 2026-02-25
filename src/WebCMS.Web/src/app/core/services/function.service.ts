import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import
  {
    FunctionDto,
    CreateFunctionRequest,
    UpdateFunctionRequest
  } from '../models/function.model';
import { PagedResult, QueryParameters } from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class FunctionService
{
  private readonly basePath = '/api/function';

  constructor(private api: ApiService) { }

  getFunctions(query: QueryParameters = {}): Observable<PagedResult<FunctionDto>>
  {
    let params = new HttpParams();

    if (query.pageNumber) params = params.set('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params = params.set('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
    if (query.includeDeleted !== undefined) params = params.set('includeDeleted', query.includeDeleted.toString());

    return this.api.get<PagedResult<FunctionDto>>(this.basePath, params);
  }

  getAllFunctions(): Observable<FunctionDto[]>
  {
    return this.api.get<FunctionDto[]>(`${this.basePath}/all`);
  }

  getMenuTree(): Observable<FunctionDto[]>
  {
    return this.api.get<FunctionDto[]>(`${this.basePath}/tree`);
  }

  getFunction(id: number): Observable<FunctionDto>
  {
    return this.api.get<FunctionDto>(`${this.basePath}/${id}`);
  }

  createFunction(request: CreateFunctionRequest): Observable<FunctionDto>
  {
    return this.api.post<FunctionDto>(this.basePath, request);
  }

  updateFunction(id: number, request: UpdateFunctionRequest): Observable<FunctionDto>
  {
    return this.api.put<FunctionDto>(`${this.basePath}/${id}`, request);
  }

  deleteFunction(id: number): Observable<void>
  {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  hardDeleteFunction(id: number): Observable<void>
  {
    return this.api.delete<void>(`${this.basePath}/${id}/permanent`);
  }

  // ============ 多語言支援方法 ============

  /**
   * 獲取功能的所有語言版本
   */
  getFunctionTranslations(id: number | string): Observable<FunctionDto[]>
  {
    return this.api.get<FunctionDto[]>(`${this.basePath}/${id}/translations`);
  }

  /**
   * 獲取功能的翻譯狀態
   */
  getFunctionTranslationStatus(id: number | string): Observable<Record<string, boolean>>
  {
    return this.api.get<Record<string, boolean>>(`${this.basePath}/${id}/translations/status`);
  }

  /**
   * 複製功能翻譯
   * @param id 功能 ID
   * @param sourceLanguage 源語言代碼
   * @param targetLanguage 目標語言代碼
   */
  copyFunctionTranslation(
    id: number | string,
    sourceLanguage: string,
    targetLanguage: string
  ): Observable<FunctionDto>
  {
    const qs = `?sourceLanguage=${encodeURIComponent(sourceLanguage)}&targetLanguage=${encodeURIComponent(targetLanguage)}`;
    return this.api.post<FunctionDto>(`${this.basePath}/${id}/translations/copy${qs}`, {});
  }

  /**
   * 刪除特定語言的功能版本
   * @param id 功能 ID
   * @param languageCode 語言代碼
   */
  deleteLanguageVersion(id: number | string, languageCode: string): Observable<void>
  {
    return this.api.delete<void>(`${this.basePath}/${id}/translations/${languageCode}`);
  }
}
