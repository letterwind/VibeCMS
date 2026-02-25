import { TranslatePipe } from './../../../core/pipes/translate.pipe';
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
import { RouterModule } from '@angular/router';
import { LanguageService } from '../../../core/services/language.service';
import { 
  DataTableComponent, 
  ColumnDefinition, 
  PaginationConfig, 
  PageEvent, 
  SortEvent, 
  RowActionEvent 
} from '../../../shared/components/data-table/data-table.component';

@Component({
  selector: 'app-article-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    DataTableComponent,
    SlidePanelComponent,
    ArticleFormComponent,
    TranslatePipe
  ],
  templateUrl: './article-list.component.html',
  styleUrl: './article-list.component.scss'
})
export class ArticleListComponent implements OnInit
{
  currentLang = 'zh-TW';
  articles: any[] = [];
  categories: CategoryDto[] = [];
  searchTerm = '';
  selectedCategoryId: number | null = null;
  isPanelOpen = false;
  isEditing = false;
  selectedArticle: ArticleDto | null = null;

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
    private articleService: ArticleService,
    private categoryService: CategoryService,
    private confirmDialog: ConfirmDialogService
    ,
    public languageService: LanguageService
  ) { }

  ngOnInit(): void
  {
    // 取得目前語言（同步值）並載入資料
    this.currentLang = this.languageService.getCurrentLanguageSync();
    this.initColumns();
    this.loadArticles();
    this.loadCategories();
  }

  private initColumns(): void {
    this.columns = [
      { key: 'id', header: 'ID', width: '80px', sortable: true },
      { key: 'title', header: this.languageService.getTranslation('article.title'), sortable: true },
      { key: 'categoryName', header: this.languageService.getTranslation('category.title'), width: '150px' },
      { key: 'tags', header: this.languageService.getTranslation('article.tags'), width: '200px' },
      { key: 'createdAt', header: this.languageService.getTranslation('label.createdAt'), width: '180px', sortable: true }
    ];
  }

  loadArticles(): void
  {
    this.articleService.getArticles(this.query, this.selectedCategoryId ?? undefined).subscribe({
      next: (result) =>
      {
        this.articles = result.items.map(article => ({
          ...article,
          tags: article.tags?.slice(0, 3).join(', ') || '',
          createdAt: new Date(article.createdAt).toLocaleString('zh-TW', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
          })
        }));
        this.pagination = {
          pageNumber: result.pageNumber,
          pageSize: result.pageSize,
          totalCount: result.totalCount,
          totalPages: result.totalPages
        };
      },
      error: (err) =>
      {
        console.error('Failed to load articles:', err);
      }
    });
  }

  loadCategories(): void
  {
    this.categoryService.getAllCategories().subscribe({
      next: (categories) =>
      {
        this.categories = categories;
      },
      error: (err) =>
      {
        console.error('Failed to load categories:', err);
      }
    });
  }

  search(): void
  {
    this.query.searchTerm = this.searchTerm;
    this.query.pageNumber = 1;
    this.loadArticles();
  }

  filterByCategory(): void
  {
    this.query.pageNumber = 1;
    this.loadArticles();
  }

  onPageChange(event: PageEvent): void {
    this.query.pageNumber = event.pageNumber;
    this.query.pageSize = event.pageSize;
    this.loadArticles();
  }

  onSort(event: SortEvent): void {
    this.query.sortBy = event.column;
    this.query.sortDescending = event.direction === 'desc';
    this.loadArticles();
  }

  onRowAction(event: RowActionEvent<ArticleDto>): void {
    if (event.action === 'edit') {
      this.openEditPanel(event.item);
    } else if (event.action === 'delete') {
      this.deleteArticle(event.item);
    }
  }

  getLevelPrefix(level: number): string
  {
    return '　'.repeat(level - 1) + (level > 1 ? '└ ' : '');
  }

  openCreatePanel(): void
  {
    this.selectedArticle = null;
    this.isEditing = false;
    this.isPanelOpen = true;
  }

  openEditPanel(article: ArticleDto): void
  {
    this.articleService.getArticle(typeof article.id === 'string' ? parseInt(article.id, 10) : article.id).subscribe({
      next: (fullArticle) =>
      {
        this.selectedArticle = fullArticle;
        this.isEditing = true;
        this.isPanelOpen = true;
      },
      error: (err) =>
      {
        console.error('Failed to load article:', err);
      }
    });
  }

  closePanel(): void
  {
    this.isPanelOpen = false;
    this.selectedArticle = null;
  }

  onSave(): void
  {
    this.closePanel();
    this.loadArticles();
  }

  deleteArticle(article: ArticleDto): void
  {
    this.confirmDialog.confirm({
      title: this.languageService.getTranslation('button.deleteArticle'),
      message: `確定要刪除文章「${article.title}」嗎？`,
      confirmText: this.languageService.getTranslation('common.delete'),
      cancelText: this.languageService.getTranslation('common.cancel'),
      type: 'danger'
    }).subscribe(confirmed =>
    {
      if (confirmed)
      {
        this.articleService.deleteArticle(typeof article.id === 'string' ? parseInt(article.id, 10) : article.id).subscribe({
          next: () =>
          {
            this.loadArticles();
          },
          error: (err) =>
          {
            console.error('Failed to delete article:', err);
          }
        });
      }
    });
  }
}
