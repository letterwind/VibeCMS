import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { ArticleDto, CreateArticleRequest, UpdateArticleRequest } from '../../../core/models/article.model';
import { CategoryDto, CategoryTreeDto } from '../../../core/models/category.model';
import { ArticleService } from '../../../core/services/article.service';
import { CategoryService } from '../../../core/services/category.service';
import { ValidationErrorComponent } from '../../../shared/components/validation-error/validation-error.component';
import { EditorComponent } from '@tinymce/tinymce-angular';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { LanguageService } from '../../../core/services/language.service';
import { TinyMceConfigService } from '../../../core/services/tinymce-config.service';

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

  form: FormGroup;
  categories: CategoryDto[] = [];
  tags: string[] = [];
  existingTags: string[] = [];
  newTag = '';
  isSubmitting = false;
  init: EditorComponent['init'];
  private editorInstance: any = null;

  constructor(
    private fb: FormBuilder,
    private articleService: ArticleService,
    private categoryService: CategoryService,
    public languageService: LanguageService,
    private tinyMceConfig: TinyMceConfigService
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
    this.init = this.tinyMceConfig.getConfig({
      setup: (editor: any) => {
        this.editorInstance = editor;
        editor.on('change', () => this.form.patchValue({ content: editor.getContent() }));
        editor.on('blur', () => this.form.patchValue({ content: editor.getContent() }));
      }
    });
  }

  ngOnDestroy(): void {
    if (this.editorInstance) {
      this.editorInstance.destroy();
      this.editorInstance = null;
    }
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
      this.editorInstance?.setContent(this.article.content || '');
    } else if (changes['article'] && !this.article) {
      this.form.reset();
      this.tags = [];
      this.editorInstance?.setContent('');
    }
  }

  private loadCategories(): void {
    this.categoryService.getCategoryTree().subscribe({
      next: (categories) => this.categories = this.flattenTree(categories),
      error: (err) => console.error('Failed to load categories:', err)
    });
  }

  private flattenTree(nodes: CategoryTreeDto[], result: CategoryDto[] = []): CategoryDto[] {
    for (const node of nodes) {
      const { children, ...dto } = node;
      result.push(dto as CategoryDto);
      if (children && children.length > 0) {
        this.flattenTree(children, result);
      }
    }
    return result;
  }

  private loadExistingTags(): void {
    this.articleService.getAllTags().subscribe({
      next: (tags) => this.existingTags = tags,
      error: (err) => console.error('Failed to load tags:', err)
    });
  }

  getLevelPrefix(level: number): string {
    return '　'.repeat(level - 1) + (level > 1 ? '└ ' : '');
  }

  addTag(event?: Event): void {
    event?.preventDefault();
    const tag = this.newTag.trim();
    if (tag && !this.tags.includes(tag)) this.tags.push(tag);
    this.newTag = '';
  }

  addExistingTag(tag: string): void {
    if (!this.tags.includes(tag)) this.tags.push(tag);
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
      this.articleService.updateArticle(
        typeof this.article.id === 'string' ? parseInt(this.article.id, 10) : this.article.id,
        request
      ).subscribe({
        next: (article) => { this.isSubmitting = false; this.onSave.emit(article); },
        error: (err) => { this.isSubmitting = false; console.error(err); alert(err.error?.message || '更新文章失敗'); }
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
        next: (article) => { this.isSubmitting = false; this.onSave.emit(article); },
        error: (err) => { this.isSubmitting = false; console.error(err); alert(err.error?.message || '建立文章失敗'); }
      });
    }
  }
}
