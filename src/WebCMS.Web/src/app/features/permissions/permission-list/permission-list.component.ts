import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoleService } from '../../../core/services/role.service';
import { PermissionService } from '../../../core/services/permission.service';
import { RoleDto } from '../../../core/models/role.model';
import {
  FunctionPermissionDto,
  PermissionSetting,
  SetPermissionsRequest
} from '../../../core/models/permission.model';

@Component({
  selector: 'app-permission-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './permission-list.component.html',
  styleUrl: './permission-list.component.scss'
})
export class PermissionListComponent implements OnInit {
  roles: RoleDto[] = [];
  selectedRoleId: number | null = null;
  functionPermissions: FunctionPermissionDto[] = [];
  flattenedFunctions: FlattenedFunction[] = [];
  saving = false;
  message = '';
  isError = false;

  constructor(
    private roleService: RoleService,
    private permissionService: PermissionService
  ) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.roleService.getAllRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
      },
      error: (err) => {
        console.error('Failed to load roles:', err);
        this.showMessage('載入角色失敗', true);
      }
    });
  }

  onRoleChange(): void {
    if (this.selectedRoleId) {
      this.loadFunctionPermissions();
    } else {
      this.functionPermissions = [];
      this.flattenedFunctions = [];
    }
  }

  loadFunctionPermissions(): void {
    if (!this.selectedRoleId) return;

    this.permissionService.getFunctionPermissions(this.selectedRoleId).subscribe({
      next: (permissions) => {
        this.functionPermissions = permissions;
        this.flattenedFunctions = this.flattenFunctions(permissions, 0);
      },
      error: (err) => {
        console.error('Failed to load permissions:', err);
        this.showMessage('載入權限失敗', true);
      }
    });
  }

  private flattenFunctions(functions: FunctionPermissionDto[], level: number): FlattenedFunction[] {
    const result: FlattenedFunction[] = [];
    for (const func of functions) {
      result.push({
        ...func,
        level
      });
      if (func.children && func.children.length > 0) {
        result.push(...this.flattenFunctions(func.children, level + 1));
      }
    }
    return result;
  }

  onPermissionChange(func: FlattenedFunction): void {
    // Mark as changed - could implement dirty tracking here
  }

  isAllSelected(func: FlattenedFunction): boolean {
    return func.canCreate && func.canRead && func.canUpdate && func.canDelete;
  }

  toggleAll(func: FlattenedFunction, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    func.canCreate = checked;
    func.canRead = checked;
    func.canUpdate = checked;
    func.canDelete = checked;
  }

  savePermissions(): void {
    if (!this.selectedRoleId) return;

    this.saving = true;
    const permissions: PermissionSetting[] = this.flattenedFunctions.map(func => ({
      functionId: func.functionId,
      canCreate: func.canCreate,
      canRead: func.canRead,
      canUpdate: func.canUpdate,
      canDelete: func.canDelete
    }));

    const request: SetPermissionsRequest = { permissions };

    this.permissionService.setPermissions(this.selectedRoleId, request).subscribe({
      next: () => {
        this.saving = false;
        this.showMessage('權限設定已儲存', false);
      },
      error: (err) => {
        this.saving = false;
        console.error('Failed to save permissions:', err);
        this.showMessage('儲存權限失敗', true);
      }
    });
  }

  private showMessage(msg: string, isError: boolean): void {
    this.message = msg;
    this.isError = isError;
    setTimeout(() => {
      this.message = '';
    }, 3000);
  }
}

interface FlattenedFunction extends FunctionPermissionDto {
  level: number;
}
