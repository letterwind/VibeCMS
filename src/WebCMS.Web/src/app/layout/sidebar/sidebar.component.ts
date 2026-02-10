import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FunctionService } from '../../core/services/function.service';
import { FunctionDto } from '../../core/models/function.model';

type DeviceType = 'mobile' | 'tablet' | 'desktop';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit {
  @Input() isOpen = false;
  @Input() deviceType: DeviceType = 'desktop';
  @Output() closeSidebar = new EventEmitter<void>();
  
  private functionService = inject(FunctionService);
  private router = inject(Router);
  
  menuItems: FunctionDto[] = [];
  private expandedMenus = new Set<number>();
  
  ngOnInit(): void {
    this.loadMenuItems();
  }
  
  loadMenuItems(): void {
    this.functionService.getMenuTree().subscribe({
      next: (items) => {
        this.menuItems = items;
        this.initializeExpandedState();
      },
      error: (err) => {
        console.error('Failed to load menu items:', err);
        this.menuItems = this.getDefaultMenuItems();
      }
    });
  }
  
  isExpanded(menuId: number): boolean {
    return this.expandedMenus.has(menuId);
  }
  
  toggleMenu(menuId: number): void {
    if (this.expandedMenus.has(menuId)) {
      this.expandedMenus.delete(menuId);
    } else {
      this.expandedMenus.add(menuId);
    }
  }
  
  onNavClick(): void {
    if (this.deviceType !== 'desktop') {
      this.closeSidebar.emit();
    }
  }
  
  private initializeExpandedState(): void {
    const currentUrl = this.router.url;
    this.menuItems.forEach(item => {
      if (item.children?.some(child => currentUrl.includes(child.url || ''))) {
        this.expandedMenus.add(item.id);
      }
    });
  }
  
  private getDefaultMenuItems(): FunctionDto[] {
    return [
      {
        id: 1,
        name: '系統管理',
        code: 'system',
        url: null,
        openInNewWindow: false,
        icon: 'bi-gear',
        parentId: null,
        sortOrder: 1,
        createdAt: new Date(),
        updatedAt: new Date(),
        children: [
          {
            id: 2,
            name: '角色管理',
            code: 'roles',
            url: '/admin/roles',
            openInNewWindow: false,
            icon: 'bi-people',
            parentId: 1,
            sortOrder: 1,
            createdAt: new Date(),
            updatedAt: new Date()
          },
          {
            id: 3,
            name: '使用者管理',
            code: 'users',
            url: '/admin/users',
            openInNewWindow: false,
            icon: 'bi-person',
            parentId: 1,
            sortOrder: 2,
            createdAt: new Date(),
            updatedAt: new Date()
          },
          {
            id: 4,
            name: '功能管理',
            code: 'functions',
            url: '/admin/functions',
            openInNewWindow: false,
            icon: 'bi-list-ul',
            parentId: 1,
            sortOrder: 3,
            createdAt: new Date(),
            updatedAt: new Date()
          },
          {
            id: 5,
            name: '權限管理',
            code: 'permissions',
            url: '/admin/permissions',
            openInNewWindow: false,
            icon: 'bi-shield-lock',
            parentId: 1,
            sortOrder: 4,
            createdAt: new Date(),
            updatedAt: new Date()
          }
        ]
      },
      {
        id: 6,
        name: '內容管理',
        code: 'content',
        url: null,
        openInNewWindow: false,
        icon: 'bi-file-earmark-text',
        parentId: null,
        sortOrder: 2,
        createdAt: new Date(),
        updatedAt: new Date(),
        children: [
          {
            id: 7,
            name: '分類管理',
            code: 'categories',
            url: '/admin/categories',
            openInNewWindow: false,
            icon: 'bi-folder',
            parentId: 6,
            sortOrder: 1,
            createdAt: new Date(),
            updatedAt: new Date()
          },
          {
            id: 8,
            name: '文章管理',
            code: 'articles',
            url: '/admin/articles',
            openInNewWindow: false,
            icon: 'bi-file-text',
            parentId: 6,
            sortOrder: 2,
            createdAt: new Date(),
            updatedAt: new Date()
          }
        ]
      },
      {
        id: 9,
        name: '網站設定',
        code: 'settings',
        url: null,
        openInNewWindow: false,
        icon: 'bi-sliders',
        parentId: null,
        sortOrder: 3,
        createdAt: new Date(),
        updatedAt: new Date(),
        children: [
          {
            id: 10,
            name: '網站設定',
            code: 'site-settings',
            url: '/admin/settings/site',
            openInNewWindow: false,
            icon: 'bi-globe',
            parentId: 9,
            sortOrder: 1,
            createdAt: new Date(),
            updatedAt: new Date()
          },
          {
            id: 11,
            name: '頁首設定',
            code: 'header-settings',
            url: '/admin/settings/header',
            openInNewWindow: false,
            icon: 'bi-layout-text-window',
            parentId: 9,
            sortOrder: 2,
            createdAt: new Date(),
            updatedAt: new Date()
          },
          {
            id: 12,
            name: '頁尾設定',
            code: 'footer-settings',
            url: '/admin/settings/footer',
            openInNewWindow: false,
            icon: 'bi-layout-text-window-reverse',
            parentId: 9,
            sortOrder: 3,
            createdAt: new Date(),
            updatedAt: new Date()
          }
        ]
      }
    ];
  }
}
