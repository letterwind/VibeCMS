import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { RoleService } from '../../../core/services/role.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { LanguageService } from '../../../core/services/language.service';
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
    UserFormComponent,
    TranslatePipe
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

  columns: ColumnDefinition[] = [];

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
    private confirmDialog: ConfirmDialogService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.initColumns();
    this.loadUsers();
    this.loadRoles();
  }

  private initColumns(): void {
    this.columns = [
      { key: 'id', header: 'ID', width: '80px', sortable: true },
      { key: 'account', header: this.languageService.getTranslation('user.account'), sortable: true },
      { key: 'displayName', header: this.languageService.getTranslation('user.displayName'), sortable: true },
      { key: 'roleNames', header: this.languageService.getTranslation('label.role') },
      { key: 'passwordStatus', header: this.languageService.getTranslation('user.passwordStatus'), width: '120px' },
      { key: 'createdAt', header: this.languageService.getTranslation('label.createdAt'), width: '180px', sortable: true }
    ];
  }

  loadUsers(): void {
    this.userService.getUsers(this.query).subscribe({
      next: (result) => {
        this.users = result.items.map(user => ({
          ...user,
          roleNames: user.roles.map(r => r.name).join(', ') || this.languageService.getTranslation('user.noRole'),
          passwordStatus: user.isPasswordExpired ? this.languageService.getTranslation('user.passwordExpired') : this.languageService.getTranslation('user.passwordNormal'),
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
      title: this.languageService.getTranslation('user.deleteUser'),
      message: this.languageService.getTranslation('user.deleteUserConfirm').replace('{0}', user.displayName),
      confirmText: this.languageService.getTranslation('common.delete'),
      cancelText: this.languageService.getTranslation('common.cancel'),
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
