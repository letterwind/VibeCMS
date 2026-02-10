export interface FunctionDto {
  id: number;
  name: string;
  code: string;
  url: string | null;
  openInNewWindow: boolean;
  icon: string | null;
  parentId: number | null;
  sortOrder: number;
  createdAt: Date;
  updatedAt: Date;
  children?: FunctionDto[];
}

export interface CreateFunctionRequest {
  name: string;
  code: string;
  url: string | null;
  openInNewWindow: boolean;
  icon: string | null;
  parentId: number | null;
  sortOrder: number;
}

export interface UpdateFunctionRequest {
  name: string;
  code: string;
  url: string | null;
  openInNewWindow: boolean;
  icon: string | null;
  parentId: number | null;
  sortOrder: number;
}
