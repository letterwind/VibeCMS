import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ArticleService } from '../../../core/services/article.service';
import { CategoryService } from '../../../core/services/category.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { SlidePanelComponent } from '../../../shared/components/slide-panel/slide-panel.component';
import { ArticleDto, QueryParameters } from '../../../core/models/article.model';
import { CategoryDto } from '../../../core/models/category.model';
import { ArticleFormComponent } from '../article-form/article-form.component';

@Component({
  selector: 'app-article-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SlidePanelComponent,
    ArticleFormComponent
  ],
  templateUrl: './article-list.component.html',
  styleUrl: './article-list.component.scss'
})
export class ArticleListComponent implements OnInit {
  articles: ArticleDto[] = [];
  categories: CategoryDto[] = [];
  searchTerm = '';
  selectedCategoryId: number | null = null;
  isPanelOpen = false;
  isEditing = false;
  isLoading = false;
  selectedArticle: ArticleDto | null = null;
  totalCount = 0;
  totalPages = 0;

  query: QueryParameters = {
    pageNumber: 1,
    pageSize: 10
  };

  constructor(
    private articleService: ArticleService,
    private categoryService: CategoryService,
    private confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.loadArticles();
    this.loadCategories();
  }

  loadArticles(): void {
    this.isLoading = true;
    this.articleService.getArticles(this.query, this.selectedCategoryId ?? undefined).subscribe({
      next: (result) => {
        this.articles = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load articles:', err);
        this.isLoading = false;
      }
    });
  }

  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (err) => {
        console.error('Failed to load categories:', err);
      }
    });
  }

  search(): void {
    this.query.searchTerm = this.searchTerm;
    this.query.pageNumber = 1;
    this.loadArticles();
  }

  filterByCategory(): void {
    this.query.pageNumber = 1;
    this.loadArticles();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.query.pageNumber = page;
      this.loadArticles();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const current = this.query.pageNumber || 1;
    const total = this.totalPages;
    
    let start = Math.max(1, current - 2);
    let end = Math.min(total, current + 2);
    
    if (end - start < 4) {
      if (start === 1) {
        end = Math.min(total, start + 4);
      } else {
        start = Math.max(1, end - 4);
      }
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  getLevelPrefix(level: number): string {
    return '　'.repeat(level - 1) + (level > 1 ? '└ ' : '');
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  openCreatePanel(): void {
    this.selectedArticle = null;
    this.isEditing = false;
    this.isPanelOpen = true;
  }

  openEditPanel(article: ArticleDto): void {
    this.articleService.getArticle(article.id).subscribe({
      next: (fullArticle) => {
        this.selectedArticle = fullArticle;
        this.isEditing = true;
        this.isPanelOpen = true;
      },
      error: (err) => {
        console.error('Failed to load article:', err);
      }
    });
  }

  closePanel(): void {
    this.isPanelOpen = false;
    this.selectedArticle = null;
  }

  onSave(article: ArticleDto): void {
    this.closePanel();
    this.loadArticles();
  }

  deleteArticle(article: ArticleDto): void {
    this.confirmDialog.confirm({
      title: '刪除文章',
      message: `確定要刪除文章「${article.title}」嗎？`,
      confirmText: '刪除',
      cancelText: '取消',
      type: 'danger'
    }).subscribe(confirmed => {
      if (confirmed) {
        this.articleService.deleteArticle(article.id).subscribe({
          next: () => {
            this.loadArticles();
          },
          error: (err) => {
            console.error('Failed to delete article:', err);
          }
        });
      }
    });
  }
}
