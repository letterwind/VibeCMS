import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  UserDto, 
  CreateUserRequest, 
  UpdateUserRequest, 
  PagedResult, 
  QueryParameters,
  ValidationResponse
} from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly basePath = '/api/user';

  constructor(private api: ApiService) {}

  getUsers(query: QueryParameters = {}): Observable<PagedResult<UserDto>> {
    let params = new HttpParams();
    
    if (query.pageNumber) params = params.set('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params = params.set('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
    if (query.includeDeleted !== undefined) params = params.set('includeDeleted', query.includeDeleted.toString());

    return this.api.get<PagedResult<UserDto>>(this.basePath, params);
  }

  getUser(id: number): Observable<UserDto> {
    return this.api.get<UserDto>(`${this.basePath}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<UserDto> {
    return this.api.post<UserDto>(this.basePath, request);
  }

  updateUser(id: number, request: UpdateUserRequest): Observable<UserDto> {
    return this.api.put<UserDto>(`${this.basePath}/${id}`, request);
  }

  deleteUser(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  hardDeleteUser(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}/permanent`);
  }

  validateAccount(account: string): Observable<ValidationResponse> {
    return this.api.post<ValidationResponse>(`${this.basePath}/validate-account`, { account });
  }

  validatePassword(password: string, account: string): Observable<ValidationResponse> {
    return this.api.post<ValidationResponse>(`${this.basePath}/validate-password`, { password, account });
  }

  isPasswordExpired(id: number): Observable<{ isExpired: boolean }> {
    return this.api.get<{ isExpired: boolean }>(`${this.basePath}/${id}/password-expired`);
  }
}
