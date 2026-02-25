import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { SlidePanelComponent } from '../../../shared/components/slide-panel/slide-panel.component';
import { CategoryDto, CategoryTreeDto, QueryParameters } from '../../../core/models/category.model';
import { CategoryFormComponent } from '../category-form/category-form.component';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SlidePanelComponent,
    CategoryFormComponent,
    TranslatePipe
  ],
  templateUrl: './category-list.component.html',
  styleUrl: './category-list.component.scss'
})
export class CategoryListComponent implements OnInit {
  categories: CategoryDto[] = [];
  categoryTree: CategoryTreeDto[] = [];
  allCategories: CategoryDto[] = [];
  searchTerm = '';
  viewMode: 'tree' | 'list' = 'tree';
  isPanelOpen = false;
  isEditing = false;
  selectedCategory: CategoryDto | null = null;
  parentCategory: CategoryDto | null = null;

  private query: QueryParameters = {
    pageNumber: 1,
    pageSize: 100
  };

  constructor(
    private categoryService: CategoryService,
    private confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.categoryService.getCategoryTree().subscribe({
      next: (result) => {
        this.categoryTree = result;
      },
      error: (err) => {
        console.error('Failed to load category tree:', err);
      }
    });

    this.categoryService.getAllCategories().subscribe({
      next: (result) => {
        this.allCategories = result;
        this.categories = result;
      },
      error: (err) => {
        console.error('Failed to load categories:', err);
      }
    });
  }

  search(): void {
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      this.categories = this.allCategories.filter(c => 
        c.name.toLowerCase().includes(term) || 
        c.slug.toLowerCase().includes(term)
      );
    } else {
      this.categories = this.allCategories;
    }
  }

  toggleView(): void {
    this.viewMode = this.viewMode === 'tree' ? 'list' : 'tree';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString('zh-TW');
  }

  openCreatePanel(parent: CategoryDto | null = null): void {
    this.selectedCategory = null;
    this.parentCategory = parent;
    this.isEditing = false;
    this.isPanelOpen = true;
  }

  openEditPanel(category: CategoryDto | CategoryTreeDto): void {
    this.categoryService.getCategory(category.id).subscribe({
      next: (fullCategory) => {
        this.selectedCategory = fullCategory;
        this.parentCategory = null;
        this.isEditing = true;
        this.isPanelOpen = true;
      },
      error: (err) => {
        console.error('Failed to load category:', err);
      }
    });
  }

  closePanel(): void {
    this.isPanelOpen = false;
    this.selectedCategory = null;
    this.parentCategory = null;
  }

  onSave(category: CategoryDto): void {
    this.closePanel();
    this.loadCategories();
  }

  deleteCategory(category: CategoryDto | CategoryTreeDto): void {
    const hasChildren = 'children' in category && category.children && category.children.length > 0;
    const message = hasChildren 
      ? `確定要刪除分類「${category.name}」嗎？此操作將同時刪除所有子分類。`
      : `確定要刪除分類「${category.name}」嗎？`;

    this.confirmDialog.confirm({
      title: '刪除分類',
      message,
      confirmText: '刪除',
      cancelText: '取消',
      type: 'danger'
    }).subscribe(confirmed => {
      if (confirmed) {
        this.categoryService.deleteCategory(category.id).subscribe({
          next: () => {
            this.loadCategories();
          },
          error: (err) => {
            console.error('Failed to delete category:', err);
          }
        });
      }
    });
  }
}
