import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  PermissionDto,
  SetPermissionsRequest,
  FunctionPermissionDto,
  PermissionType
} from '../models/permission.model';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private readonly basePath = '/permission';

  constructor(private api: ApiService) {}

  /**
   * 取得角色的所有權限
   */
  getPermissionsByRole(roleId: number): Observable<PermissionDto[]> {
    return this.api.get<PermissionDto[]>(`${this.basePath}/role/${roleId}`);
  }

  /**
   * 設定角色的權限
   */
  setPermissions(roleId: number, request: SetPermissionsRequest): Observable<void> {
    return this.api.post<void>(`${this.basePath}/role/${roleId}`, request);
  }

  /**
   * 取得角色的功能權限樹狀結構
   */
  getFunctionPermissions(roleId: number): Observable<FunctionPermissionDto[]> {
    return this.api.get<FunctionPermissionDto[]>(`${this.basePath}/role/${roleId}/functions`);
  }

  /**
   * 取得使用者的所有權限
   */
  getUserPermissions(userId: number): Observable<PermissionDto[]> {
    return this.api.get<PermissionDto[]>(`${this.basePath}/user/${userId}`);
  }

  /**
   * 檢查使用者是否有特定功能的權限
   */
  checkPermission(userId: number, functionCode: string, type: PermissionType): Observable<boolean> {
    const params = new HttpParams()
      .set('userId', userId.toString())
      .set('functionCode', functionCode)
      .set('type', type);
    return this.api.get<boolean>(`${this.basePath}/check`, params);
  }
}
