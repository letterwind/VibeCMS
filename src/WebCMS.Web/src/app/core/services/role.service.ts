import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  RoleDto, 
  CreateRoleRequest, 
  UpdateRoleRequest, 
  PagedResult, 
  QueryParameters 
} from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private readonly basePath = '/api/role';

  constructor(private api: ApiService) {}

  getRoles(query: QueryParameters = {}): Observable<PagedResult<RoleDto>> {
    let params = new HttpParams();
    
    if (query.pageNumber) params = params.set('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params = params.set('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
    if (query.includeDeleted !== undefined) params = params.set('includeDeleted', query.includeDeleted.toString());

    return this.api.get<PagedResult<RoleDto>>(this.basePath, params);
  }

  getAllRoles(): Observable<RoleDto[]> {
    return this.api.get<RoleDto[]>(`${this.basePath}/all`);
  }

  getRole(id: number): Observable<RoleDto> {
    return this.api.get<RoleDto>(`${this.basePath}/${id}`);
  }

  createRole(request: CreateRoleRequest): Observable<RoleDto> {
    return this.api.post<RoleDto>(this.basePath, request);
  }

  updateRole(id: number, request: UpdateRoleRequest): Observable<RoleDto> {
    return this.api.put<RoleDto>(`${this.basePath}/${id}`, request);
  }

  deleteRole(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  hardDeleteRole(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}/permanent`);
  }
}
