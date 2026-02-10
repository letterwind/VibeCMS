export interface PermissionDto {
  functionId: number;
  functionName: string;
  functionCode: string;
  canCreate: boolean;
  canRead: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}

export interface PermissionSetting {
  functionId: number;
  canCreate: boolean;
  canRead: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}

export interface SetPermissionsRequest {
  permissions: PermissionSetting[];
}

export interface FunctionPermissionDto {
  functionId: number;
  functionName: string;
  functionCode: string;
  icon: string | null;
  parentId: number | null;
  sortOrder: number;
  canCreate: boolean;
  canRead: boolean;
  canUpdate: boolean;
  canDelete: boolean;
  children: FunctionPermissionDto[] | null;
}

export enum PermissionType {
  Create = 'Create',
  Read = 'Read',
  Update = 'Update',
  Delete = 'Delete'
}
