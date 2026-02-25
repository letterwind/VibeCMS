import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { ArticleDto, CreateArticleRequest, UpdateArticleRequest } from '../../../core/models/article.model';
import { CategoryDto } from '../../../core/models/category.model';
import { ArticleService } from '../../../core/services/article.service';
import { CategoryService } from '../../../core/services/category.service';
import { ValidationErrorComponent } from '../../../shared/components/validation-error/validation-error.component';
import { EditorComponent } from '@tinymce/tinymce-angular';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { LanguageService } from '../../../core/services/language.service';

declare const tinymce: any;

@Component({
  selector: 'app-article-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, ValidationErrorComponent, EditorComponent, TranslatePipe],
  templateUrl: './article-form.component.html',
  styleUrl: './article-form.component.scss'
})
export class ArticleFormComponent implements OnChanges, OnInit, OnDestroy, AfterViewInit {
  @Input() article: ArticleDto | null = null;
  @Input() isEditing = false;
  @Output() onSave = new EventEmitter<ArticleDto>();
  @Output() onCancel = new EventEmitter<void>();
  @ViewChild('contentEditor') contentEditor!: ElementRef;

  form: FormGroup;
  categories: CategoryDto[] = [];
  tags: string[] = [];
  existingTags: string[] = [];
  newTag = '';
  isSubmitting = false;
  private editorInstance: any = null;

  init: EditorComponent['init'] = {
    base_url: '/assets/js/tinymce',
    suffix: '.min',
    height: 400,
    menubar: false,
    plugins: [
      'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
      'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
      'insertdatetime', 'media', 'table', 'help', 'wordcount'
    ],
    toolbar: 'undo redo | blocks | bold italic forecolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | removeformat | link image | code | help',
    content_style: 'body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; font-size: 14px; }'
  };

  constructor(
    private fb: FormBuilder,
    private articleService: ArticleService,
    private categoryService: CategoryService,
    public languageService: LanguageService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      slug: ['', [Validators.required, Validators.maxLength(200)]],
      categoryId: ['', Validators.required],
      content: ['', Validators.required],
      metaTitle: ['', Validators.maxLength(100)],
      metaDescription: ['', Validators.maxLength(200)],
      metaKeywords: ['', Validators.maxLength(200)]
    });
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadExistingTags();
  }

  ngAfterViewInit(): void {
    this.initTinyMCE();
  }

  ngOnDestroy(): void {
    this.destroyTinyMCE();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['article'] && this.article) {
      this.form.patchValue({
        title: this.article.title,
        slug: this.article.slug,
        categoryId: this.article.categoryId,
        content: this.article.content,
        metaTitle: this.article.metaTitle || '',
        metaDescription: this.article.metaDescription || '',
        metaKeywords: this.article.metaKeywords || ''
      });
      this.tags = [...(this.article.tags || [])];
      
      if (this.editorInstance) {
        this.editorInstance.setContent(this.article.content || '');
      }
    } else if (changes['article'] && !this.article) {
      this.form.reset();
      this.tags = [];
      if (this.editorInstance) {
        this.editorInstance.setContent('');
      }
    }
  }

  private initTinyMCE(): void {
    this.init = {
      height: 400,
      menubar: false,
      plugins: [
        'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
        'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
        'insertdatetime', 'media', 'table', 'help', 'wordcount'
      ],
      toolbar: 'undo redo | blocks | bold italic forecolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | removeformat | link image | code | help',
      content_style: 'body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; font-size: 14px; }',
      setup: (editor: any) => {
        this.editorInstance = editor;
        editor.on('change', () => {
          this.form.patchValue({ content: editor.getContent() });
        });
        editor.on('blur', () => {
          this.form.patchValue({ content: editor.getContent() });
        });
      }
    }
  }

  private destroyTinyMCE(): void {
    if (this.editorInstance) {
      this.editorInstance.destroy();
      this.editorInstance = null;
    }
  }

  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (err) => {
        console.error('Failed to load categories:', err);
      }
    });
  }

  private loadExistingTags(): void {
    this.articleService.getAllTags().subscribe({
      next: (tags) => {
        this.existingTags = tags;
      },
      error: (err) => {
        console.error('Failed to load tags:', err);
      }
    });
  }

  getLevelPrefix(level: number): string {
    return '　'.repeat(level - 1) + (level > 1 ? '└ ' : '');
  }

  addTag(event?: Event): void {
    if (event) {
      event.preventDefault();
    }
    const tag = this.newTag.trim();
    if (tag && !this.tags.includes(tag)) {
      this.tags.push(tag);
    }
    this.newTag = '';
  }

  addExistingTag(tag: string): void {
    if (!this.tags.includes(tag)) {
      this.tags.push(tag);
    }
  }

  removeTag(tag: string): void {
    this.tags = this.tags.filter(t => t !== tag);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.editorInstance) {
      this.form.patchValue({ content: this.editorInstance.getContent() });
    }

    this.isSubmitting = true;
    const formValue = this.form.value;

    if (this.isEditing && this.article) {
      const request: UpdateArticleRequest = {
        title: formValue.title,
        content: formValue.content,
        slug: formValue.slug,
        categoryId: parseInt(formValue.categoryId, 10),
        tags: this.tags.length > 0 ? this.tags : null,
        metaTitle: formValue.metaTitle || null,
        metaDescription: formValue.metaDescription || null,
        metaKeywords: formValue.metaKeywords || null
      };

      this.articleService.updateArticle(typeof this.article.id === 'string' ? parseInt(this.article.id, 10) : this.article.id, request).subscribe({
        next: (article) => {
          this.isSubmitting = false;
          this.onSave.emit(article);
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Failed to update article:', err);
          alert(err.error?.message || '更新文章失敗');
        }
      });
    } else {
      const request: CreateArticleRequest = {
        title: formValue.title,
        content: formValue.content,
        slug: formValue.slug,
        categoryId: parseInt(formValue.categoryId, 10),
        tags: this.tags.length > 0 ? this.tags : null,
        metaTitle: formValue.metaTitle || null,
        metaDescription: formValue.metaDescription || null,
        metaKeywords: formValue.metaKeywords || null
      };

      this.articleService.createArticle(request).subscribe({
        next: (article) => {
          this.isSubmitting = false;
          this.onSave.emit(article);
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Failed to create article:', err);
          alert(err.error?.message || '建立文章失敗');
        }
      });
    }
  }
}
