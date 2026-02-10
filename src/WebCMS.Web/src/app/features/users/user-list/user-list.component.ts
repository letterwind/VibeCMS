import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { RoleService } from '../../../core/services/role.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { 
  DataTableComponent, 
  ColumnDefinition, 
  PaginationConfig, 
  PageEvent, 
  SortEvent, 
  RowActionEvent 
} from '../../../shared/components/data-table/data-table.component';
import { SlidePanelComponent } from '../../../shared/components/slide-panel/slide-panel.component';
import { UserDto, QueryParameters } from '../../../core/models/user.model';
import { RoleDto } from '../../../core/models/role.model';
import { UserFormComponent } from '../user-form/user-form.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DataTableComponent,
    SlidePanelComponent,
    UserFormComponent
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements OnInit {
  users: any[] = [];
  allRoles: RoleDto[] = [];
  searchTerm = '';
  isPanelOpen = false;
  isEditing = false;
  selectedUser: UserDto | null = null;

  columns: ColumnDefinition[] = [
    { key: 'id', header: 'ID', width: '80px', sortable: true },
    { key: 'account', header: '帳號', sortable: true },
    { key: 'displayName', header: '顯示名稱', sortable: true },
    { key: 'roleNames', header: '角色' },
    { key: 'passwordStatus', header: '密碼狀態', width: '120px' },
    { key: 'createdAt', header: '建立時間', width: '180px', sortable: true }
  ];

  pagination: PaginationConfig = {
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  };

  private query: QueryParameters = {
    pageNumber: 1,
    pageSize: 10
  };

  constructor(
    private userService: UserService,
    private roleService: RoleService,
    private confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
  }

  loadUsers(): void {
    this.userService.getUsers(this.query).subscribe({
      next: (result) => {
        this.users = result.items.map(user => ({
          ...user,
          roleNames: user.roles.map(r => r.name).join(', ') || '無',
          passwordStatus: user.isPasswordExpired ? '已過期' : '正常',
          createdAt: new Date(user.createdAt).toLocaleString('zh-TW')
        }));
        this.pagination = {
          pageNumber: result.pageNumber,
          pageSize: result.pageSize,
          totalCount: result.totalCount,
          totalPages: result.totalPages
        };
      },
      error: (err) => {
        console.error('Failed to load users:', err);
      }
    });
  }

  loadRoles(): void {
    this.roleService.getAllRoles().subscribe({
      next: (roles) => {
        this.allRoles = roles;
      },
      error: (err) => {
        console.error('Failed to load roles:', err);
      }
    });
  }

  search(): void {
    this.query.searchTerm = this.searchTerm;
    this.query.pageNumber = 1;
    this.loadUsers();
  }

  onPageChange(event: PageEvent): void {
    this.query.pageNumber = event.pageNumber;
    this.query.pageSize = event.pageSize;
    this.loadUsers();
  }

  onSort(event: SortEvent): void {
    this.query.sortBy = event.column;
    this.query.sortDescending = event.direction === 'desc';
    this.loadUsers();
  }

  onRowAction(event: RowActionEvent<UserDto>): void {
    if (event.action === 'edit') {
      this.openEditPanel(event.item);
    } else if (event.action === 'delete') {
      this.deleteUser(event.item);
    }
  }

  openCreatePanel(): void {
    this.selectedUser = null;
    this.isEditing = false;
    this.isPanelOpen = true;
  }

  openEditPanel(user: UserDto): void {
    this.userService.getUser(user.id).subscribe({
      next: (fullUser) => {
        this.selectedUser = fullUser;
        this.isEditing = true;
        this.isPanelOpen = true;
      },
      error: (err) => {
        console.error('Failed to load user:', err);
      }
    });
  }

  closePanel(): void {
    this.isPanelOpen = false;
    this.selectedUser = null;
  }

  onSave(user: UserDto): void {
    this.closePanel();
    this.loadUsers();
  }

  deleteUser(user: UserDto): void {
    this.confirmDialog.confirm({
      title: '刪除使用者',
      message: `確定要刪除使用者「${user.displayName}」嗎？此操作將執行軟刪除。`,
      confirmText: '刪除',
      cancelText: '取消',
      type: 'danger'
    }).subscribe(confirmed => {
      if (confirmed) {
        this.userService.deleteUser(user.id).subscribe({
          next: () => {
            this.loadUsers();
          },
          error: (err) => {
            console.error('Failed to delete user:', err);
          }
        });
      }
    });
  }
}
