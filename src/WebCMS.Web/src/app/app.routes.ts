import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { languageGuard } from './core/guards/language.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'zh-TW/admin',
    pathMatch: 'full'
  },
  {
    path: ':lang',
    canActivate: [languageGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'admin',
        loadComponent: () => import('./layout/layout.component').then(m => m.LayoutComponent),
        canActivate: [authGuard],
        children: [
          {
            path: '',
            redirectTo: 'dashboard',
            pathMatch: 'full'
          },
          {
            path: 'dashboard',
            loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
          },
          {
            path: 'language-edit',
            loadComponent: () => import('./admin/components/language-resource-editor/language-resource-editor.component').then(m=>m.LanguageResourceEditorComponent)
          },
          {
            path: 'roles',
            loadComponent: () => import('./features/roles/role-list/role-list.component').then(m => m.RoleListComponent)
          },
          {
            path: 'functions',
            loadComponent: () => import('./features/functions/function-list/function-list.component').then(m => m.FunctionListComponent)
          },
          {
            path: 'permissions',
            loadComponent: () => import('./features/permissions/permission-list/permission-list.component').then(m => m.PermissionListComponent)
          },
          {
            path: 'users',
            loadComponent: () => import('./features/users/user-list/user-list.component').then(m => m.UserListComponent)
          },
          {
            path: 'categories',
            loadComponent: () => import('./features/categories/category-list/category-list.component').then(m => m.CategoryListComponent)
          },
          {
            path: 'articles',
            loadComponent: () => import('./features/articles/article-list/article-list.component').then(m => m.ArticleListComponent)
          },
          {
            path: 'articles/:id/edit',
            loadComponent: () => import('./features/articles/article-multi-language-edit/article-multi-language-edit.component').then(m => m.ArticleMultiLanguageEditComponent)
          },
          {
            path: 'settings/site',
            loadComponent: () => import('./features/settings/site-settings/site-settings.component').then(m => m.SiteSettingsComponent)
          },
          {
            path: 'settings/header',
            loadComponent: () => import('./features/settings/header-settings/header-settings.component').then(m => m.HeaderSettingsComponent)
          },
          {
            path: 'settings/footer',
            loadComponent: () => import('./features/settings/footer-settings/footer-settings.component').then(m => m.FooterSettingsComponent)
          }
        ]
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'zh-TW/admin'
  }
];
