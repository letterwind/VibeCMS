import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subject, takeUntil } from 'rxjs';

import { LanguageService } from '../../../core/services/language.service';
import { CategoryService } from '../../../core/services/category.service';
import { CategoryDto } from '../../../core/models/category.model';
import { Language } from '../../../core/models/language.model';
import { ArticleService } from '../../../core/services/article.service';
import { ArticleDto, UpdateArticleRequest } from '../../../core/models/article.model';
import { LanguageSelectorComponent } from '../../../shared/components/language-selector/language-selector.component';

/**
 * 多語言文章編輯組件
 * 支持在多個語言中編輯文章內容，以及複製和刪除語言版本
 */
@Component({
  selector: 'app-article-multi-language-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, LanguageSelectorComponent],
  template: `
    <div class="article-editor-container">
      <div class="editor-header">
        <h2>編輯文章 - 多語言支援</h2>
        <app-language-selector></app-language-selector>
      </div>

      <div class="language-tabs" *ngIf="languages$ | async as languages">
        <div class="tab-buttons">
          <button *ngFor="let lang of languages"
                  [class.active]="selectedLanguage === lang.languageCode"
                  (click)="selectLanguage(lang.languageCode)"
                  class="tab-button">
            {{ lang.languageName }}
            <span class="translation-status" [class.translated]="translationStatus[lang.languageCode]">
              {{ translationStatus[lang.languageCode] ? '✓' : '-' }}
            </span>
          </button>
        </div>

        <div class="tab-content">
          <form [formGroup]="articleForm" (ngSubmit)="onSave()">
            <div class="form-group">
              <label for="title">標題</label>
              <input type="text" 
                     id="title" 
                     formControlName="title" 
                     class="form-control"
                     required>
            </div>

            <div class="form-group">
              <label for="slug">網址別名</label>
              <input type="text" 
                     id="slug" 
                     formControlName="slug" 
                     class="form-control"
                     required>
            </div>

            <div class="form-group">
              <label for="category">分類</label>
              <select id="category" class="form-control" formControlName="categoryId">
                <option [ngValue]="null">未選擇</option>
                <option *ngFor="let c of categories" [ngValue]="c.id">{{ c.name }}</option>
              </select>
            </div>

            <div class="form-group">
              <label for="content">內容</label>
              <textarea id="content" 
                        formControlName="content" 
                        class="form-control"
                        rows="10"
                        required></textarea>
            </div>

            <div class="form-group">
              <label for="summary">摘要</label>
              <textarea id="summary" 
                        formControlName="summary" 
                        class="form-control"
                        rows="3"></textarea>
            </div>

            <div class="form-actions">
              <button type="submit" class="btn btn-primary">保存</button>
              <button type="button" class="btn btn-secondary" (click)="onCancel()">取消</button>
              <button type="button" class="btn btn-warning" (click)="onCopyTranslation()">複製翻譯</button>
              <button type="button" class="btn btn-danger" (click)="onDeleteVersion()">刪除版本</button>
            </div>
          </form>
        </div>
      </div>

      <!-- 複製翻譯對話框 -->
      <div class="modal" *ngIf="showCopyDialog">
        <div class="modal-content">
          <h3>複製翻譯</h3>
          <p>從 {{ sourceLanguage }} 複製到:</p>
          <select [(ngModel)]="targetLanguage" class="form-control">
            <option *ngFor="let lang of languages$ | async" 
                    [value]="lang.languageCode"
                    [disabled]="lang.languageCode === selectedLanguage">
              {{ lang.languageName }}
            </option>
          </select>
          <div class="modal-actions">
            <button (click)="confirmCopyTranslation()" class="btn btn-primary">確認</button>
            <button (click)="showCopyDialog = false" class="btn btn-secondary">取消</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .article-editor-container {
      padding: 20px;
    }

    .editor-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .language-tabs {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .tab-buttons {
      display: flex;
      gap: 10px;
      border-bottom: 2px solid #e0e0e0;
      padding: 10px;
      flex-wrap: wrap;
    }

    .tab-button {
      padding: 8px 16px;
      background: #f5f5f5;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      transition: all 0.3s;
      display: flex;
      align-items: center;
      gap: 5px;
    }

    .tab-button.active {
      background: #007bff;
      color: white;
      font-weight: bold;
    }

    .tab-button:hover {
      background: #e0e0e0;
    }

    .tab-button.active:hover {
      background: #0056b3;
    }

    .translation-status {
      font-size: 12px;
      padding: 2px 4px;
      background: rgba(0,0,0,0.1);
      border-radius: 2px;
    }

    .translation-status.translated {
      background: #28a745;
      color: white;
    }

    .tab-content {
      padding: 20px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    .form-group label {
      display: block;
      margin-bottom: 5px;
      font-weight: bold;
    }

    .form-group textarea,
    .form-group input {
      padding: 8px;
      border: 1px solid #ddd;
      border-radius: 4px;
    }

    .form-actions {
      display: flex;
      gap: 10px;
      margin-top: 20px;
    }

    .btn {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
    }

    .btn-warning {
      background: #ffc107;
      color: black;
    }

    .btn-danger {
      background: #dc3545;
      color: white;
    }

    .modal {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .modal-content {
      background: white;
      padding: 20px;
      border-radius: 8px;
      max-width: 400px;
      width: 90%;
    }

    .modal-actions {
      display: flex;
      gap: 10px;
      margin-top: 20px;
    }
  `]
})
export class ArticleMultiLanguageEditComponent implements OnInit, OnDestroy
{
  languages$: Observable<Language[]>;

  selectedLanguage: string = 'zh-TW';
  sourceLanguage: string = 'zh-TW';
  targetLanguage: string = 'en-US';

  articleForm: FormGroup;
  articles: Map<string, ArticleDto> = new Map();
  translationStatus: Record<string, boolean> = {};
  categories: CategoryDto[] = [];

  showCopyDialog = false;
  private destroy$ = new Subject<void>();

  private articleId: string = '';

  constructor(
    private fb: FormBuilder,
    private languageService: LanguageService,
    private articleService: ArticleService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router
  )
  {
    this.languages$ = this.languageService.getLanguages();

    this.articleForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      slug: ['', [Validators.required, Validators.minLength(3)]],
      content: ['', [Validators.required]],
      summary: [''],
      categoryId: [null]
    });
  }

  ngOnInit(): void
  {
    // 從路由參數獲取文章 ID
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params =>
    {
      this.articleId = params.get('id') || '';
      if (this.articleId)
      {
        this.loadArticleTranslations();
      }
    });

    // 載入分類供下拉選單使用
    this.loadCategories();

    // 當語言變化時，更新表單
    this.languageService.getCurrentLanguage()
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang =>
      {
        this.selectedLanguage = lang;
        this.loadArticleForLanguage(lang);
      });
  }

  ngOnDestroy(): void
  {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 加載所有語言的文章版本
   */
  private loadArticleTranslations(): void
  {
    this.articleService.getArticleTranslations(this.articleId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (articles: ArticleDto[]) =>
        {
          // 將所有語言版本存儲到 Map 中
          articles.forEach(article =>
          {
            this.articles.set(article.languageCode, article);
          });

          // 加載翻譯狀態
          this.loadTranslationStatus();
        },
        error: (error) =>
        {
          console.error('加載文章翻譯失敗:', error);
          // 如果失敗，至少載入默認語言版本
          this.loadDefaultLanguageArticle();
        }
      });
  }

  /**
   * 加載翻譯狀態
   */
  private loadTranslationStatus(): void
  {
    this.articleService.getArticleTranslationStatus(this.articleId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (status: Record<string, boolean>) =>
        {
          this.translationStatus = status;
          // 加載當前語言的文章內容
          this.loadArticleForLanguage(this.selectedLanguage);
        },
        error: (error) =>
        {
          console.error('加載翻譯狀態失敗:', error);
          this.loadArticleForLanguage(this.selectedLanguage);
        }
      });
  }

  /**
   * 加載默認語言的文章（用於錯誤恢復）
   */
  private loadDefaultLanguageArticle(): void
  {
    this.articleService.getArticle(parseInt(this.articleId, 10))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (article: ArticleDto) =>
        {
          this.articles.set(article.languageCode, article);
          this.loadArticleForLanguage(article.languageCode);
        },
        error: (error) =>
        {
          console.error('加載默認語言文章失敗:', error);
        }
      });
  }

  /**
   * 加載特定語言的文章
   */
  private loadArticleForLanguage(lang: string): void
  {
    if (this.articles.has(lang))
    {
      const article = this.articles.get(lang);
      if (article)
      {
        this.articleForm.patchValue({
          title: article.title,
          slug: article.slug,
          content: article.content,
          summary: article.summary,
          categoryId: article.categoryId
        });
      }
    } else
    {
      this.articleForm.reset();
    }
  }

  /**
   * 選擇語言
   */
  selectLanguage(lang: string): void
  {
    this.languageService.setCurrentLanguage(lang);
  }

  /**
   * 保存文章
   */
  onSave(): void
  {
    if (this.articleForm.valid)
    {
      const formData = this.articleForm.value;
      const articleId = parseInt(this.articleId, 10);
      const updateRequest: UpdateArticleRequest = {
        title: formData.title,
        slug: formData.slug,
        content: formData.content,
        categoryId: Number(formData.categoryId ?? 0),
        tags: formData.tags ?? this.articles.get(this.selectedLanguage)?.tags ?? [],
        metaTitle: null,
        metaDescription: null,
        metaKeywords: null
      };

      this.articleService.updateArticle(articleId, updateRequest)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updatedArticle: ArticleDto) =>
          {
            // 更新本地存儲
            this.articles.set(this.selectedLanguage, updatedArticle);
            console.log('文章已保存:', updatedArticle);
            // 可選：顯示成功提示
            // this.toastr.success('文章已保存');
          },
          error: (error) =>
          {
            console.error('保存文章失敗:', error);
            // 顯示錯誤提示
            // this.toastr.error('保存文章失敗');
          }
        });
    }
  }

  private loadCategories(): void
  {
    this.categoryService.getAllCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (cats) => this.categories = cats,
        error: (err) => console.error('載入分類失敗:', err)
      });
  }

  /**
   * 取消編輯
   */
  onCancel(): void
  {
    this.router.navigate([this.selectedLanguage, 'admin', 'articles']);
  }

  /**
   * 打開複製翻譯對話框
   */
  onCopyTranslation(): void
  {
    this.sourceLanguage = this.selectedLanguage;
    this.showCopyDialog = true;
  }

  /**
   * 確認複製翻譯
   */
  confirmCopyTranslation(): void
  {
    const articleId = parseInt(this.articleId, 10);
    this.articleService.copyArticleTranslation(
      articleId,
      this.sourceLanguage,
      this.targetLanguage
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (copiedArticle: ArticleDto) =>
        {
          // 更新本地存儲和翻譯狀態
          this.articles.set(this.targetLanguage, copiedArticle);
          this.translationStatus[this.targetLanguage] = true;
          this.showCopyDialog = false;
          console.log('翻譯已複製:', copiedArticle);
          // 顯示成功提示
          // this.toastr.success('翻譯已複製');
        },
        error: (error) =>
        {
          console.error('複製翻譯失敗:', error);
          // 顯示錯誤提示
          // this.toastr.error('複製翻譯失敗');
        }
      });
  }

  /**
   * 刪除當前語言版本
   */
  onDeleteVersion(): void
  {
    if (confirm(`確定要刪除 ${this.selectedLanguage} 的版本嗎？`))
    {
      const articleId = parseInt(this.articleId, 10);
      this.articleService.deleteLanguageVersion(articleId, this.selectedLanguage)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () =>
          {
            // 更新本地存儲
            this.articles.delete(this.selectedLanguage);
            this.translationStatus[this.selectedLanguage] = false;
            this.articleForm.reset();
            console.log('語言版本已刪除');
            // 顯示成功提示
            // this.toastr.success('語言版本已刪除');
          },
          error: (error) =>
          {
            console.error('刪除語言版本失敗:', error);
            // 顯示錯誤提示
            // this.toastr.error('刪除語言版本失敗');
          }
        });
    }
  }
}
