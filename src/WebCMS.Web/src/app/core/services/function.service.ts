import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  FunctionDto, 
  CreateFunctionRequest, 
  UpdateFunctionRequest 
} from '../models/function.model';
import { PagedResult, QueryParameters } from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class FunctionService {
  private readonly basePath = '/api/function';

  constructor(private api: ApiService) {}

  getFunctions(query: QueryParameters = {}): Observable<PagedResult<FunctionDto>> {
    let params = new HttpParams();
    
    if (query.pageNumber) params = params.set('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params = params.set('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
    if (query.includeDeleted !== undefined) params = params.set('includeDeleted', query.includeDeleted.toString());

    return this.api.get<PagedResult<FunctionDto>>(this.basePath, params);
  }

  getAllFunctions(): Observable<FunctionDto[]> {
    return this.api.get<FunctionDto[]>(`${this.basePath}/all`);
  }

  getMenuTree(): Observable<FunctionDto[]> {
    return this.api.get<FunctionDto[]>(`${this.basePath}/tree`);
  }

  getFunction(id: number): Observable<FunctionDto> {
    return this.api.get<FunctionDto>(`${this.basePath}/${id}`);
  }

  createFunction(request: CreateFunctionRequest): Observable<FunctionDto> {
    return this.api.post<FunctionDto>(this.basePath, request);
  }

  updateFunction(id: number, request: UpdateFunctionRequest): Observable<FunctionDto> {
    return this.api.put<FunctionDto>(`${this.basePath}/${id}`, request);
  }

  deleteFunction(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  hardDeleteFunction(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}/permanent`);
  }
}
