import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
import { RoleDto, QueryParameters } from '../../../core/models/role.model';
import { RoleFormComponent } from '../role-form/role-form.component';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DataTableComponent,
    SlidePanelComponent,
    RoleFormComponent,
    TranslatePipe
  ],
  templateUrl: './role-list.component.html',
  styleUrl: './role-list.component.scss'
})
export class RoleListComponent implements OnInit {
  roles: RoleDto[] = [];
  searchTerm = '';
  isPanelOpen = false;
  isEditing = false;
  selectedRole: RoleDto | null = null;

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
    private roleService: RoleService,
    private confirmDialog: ConfirmDialogService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.initColumns();
    this.loadRoles();
  }

  private initColumns(): void {
    this.columns = [
      { key: 'id', header: 'ID', width: '80px', sortable: true },
      { key: 'name', header: this.languageService.getTranslation('role.name'), sortable: true },
      { key: 'description', header: this.languageService.getTranslation('label.description') },
      { key: 'hierarchyLevel', header: this.languageService.getTranslation('role.hierarchyLevel'), width: '100px', sortable: true },
      { key: 'createdAt', header: this.languageService.getTranslation('label.createdAt'), width: '180px', sortable: true }
    ];
  }

  loadRoles(): void {
    this.roleService.getRoles(this.query).subscribe({
      next: (result) => {
        this.roles = result.items.map(role => ({
          ...role,
          createdAt: new Date(role.createdAt).toLocaleString('zh-TW')
        })) as any;
        this.pagination = {
          pageNumber: result.pageNumber,
          pageSize: result.pageSize,
          totalCount: result.totalCount,
          totalPages: result.totalPages
        };
      },
      error: (err) => {
        console.error('Failed to load roles:', err);
      }
    });
  }

  search(): void {
    this.query.searchTerm = this.searchTerm;
    this.query.pageNumber = 1;
    this.loadRoles();
  }

  onPageChange(event: PageEvent): void {
    this.query.pageNumber = event.pageNumber;
    this.query.pageSize = event.pageSize;
    this.loadRoles();
  }

  onSort(event: SortEvent): void {
    this.query.sortBy = event.column;
    this.query.sortDescending = event.direction === 'desc';
    this.loadRoles();
  }

  onRowAction(event: RowActionEvent<RoleDto>): void {
    if (event.action === 'edit') {
      this.openEditPanel(event.item);
    } else if (event.action === 'delete') {
      this.deleteRole(event.item);
    }
  }

  openCreatePanel(): void {
    this.selectedRole = null;
    this.isEditing = false;
    this.isPanelOpen = true;
  }

  openEditPanel(role: RoleDto): void {
    this.roleService.getRole(role.id).subscribe({
      next: (fullRole) => {
        this.selectedRole = fullRole;
        this.isEditing = true;
        this.isPanelOpen = true;
      },
      error: (err) => {
        console.error('Failed to load role:', err);
      }
    });
  }

  closePanel(): void {
    this.isPanelOpen = false;
    this.selectedRole = null;
  }

  onSave(role: RoleDto): void {
    this.closePanel();
    this.loadRoles();
  }

  deleteRole(role: RoleDto): void {
    this.confirmDialog.confirm({
      title: this.languageService.getTranslation('role.deleteRole'),
      message: this.languageService.getTranslation('role.deleteRoleConfirm').replace('{0}', role.name),
      confirmText: this.languageService.getTranslation('common.delete'),
      cancelText: this.languageService.getTranslation('common.cancel'),
      type: 'danger'
    }).subscribe(confirmed => {
      if (confirmed) {
        this.roleService.deleteRole(role.id).subscribe({
          next: () => {
            this.loadRoles();
          },
          error: (err) => {
            console.error('Failed to delete role:', err);
          }
        });
      }
    });
  }
}
