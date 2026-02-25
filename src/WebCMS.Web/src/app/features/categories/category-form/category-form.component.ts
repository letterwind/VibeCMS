import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { CategoryDto, CreateCategoryRequest, UpdateCategoryRequest } from '../../../core/models/category.model';
import { ValidationErrorComponent } from '../../../shared/components/validation-error/validation-error.component';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ValidationErrorComponent, TranslatePipe],
  templateUrl: './category-form.component.html',
  styleUrl: './category-form.component.scss'
})
export class CategoryFormComponent implements OnInit, OnChanges {
  @Input() category: CategoryDto | null = null;
  @Input() parentCategory: CategoryDto | null = null;
  @Input() allCategories: CategoryDto[] = [];
  @Input() isEditing = false;
  @Output() onSave = new EventEmitter<CategoryDto>();
  @Output() onCancel = new EventEmitter<void>();

  form!: FormGroup;
  isSubmitting = false;
  availableParents: { id: number; name: string; level: number; indent: string }[] = [];

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    public languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['category'] || changes['parentCategory'] || changes['allCategories']) && this.form) {
      this.updateForm();
      this.updateAvailableParents();
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(20)]],
      slug: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/^[a-z0-9-]+$/)]],
      parentId: [null],
      sortOrder: [0, [Validators.min(0)]],
      metaTitle: ['', [Validators.maxLength(100)]],
      metaDescription: ['', [Validators.maxLength(200)]],
      metaKeywords: ['', [Validators.maxLength(200)]]
    });
    this.updateForm();
    this.updateAvailableParents();
  }

  private updateForm(): void {
    if (this.category) {
      this.form.patchValue({
        name: this.category.name,
        slug: this.category.slug,
        parentId: this.category.parentId,
        sortOrder: this.category.sortOrder,
        metaTitle: this.category.metaTitle || '',
        metaDescription: this.category.metaDescription || '',
        metaKeywords: this.category.metaKeywords || ''
      });
    } else {
      this.form.reset({
        name: '',
        slug: '',
        parentId: this.parentCategory?.id || null,
        sortOrder: 0,
        metaTitle: '',
        metaDescription: '',
        metaKeywords: ''
      });
    }
  }

  private updateAvailableParents(): void {
    const tree = this.buildTree(this.allCategories);
    const flattened = this.flattenTree(tree, 0);
    
    this.availableParents = flattened.filter(item => {
      if (item.level >= 3) return false;
      if (this.category && item.id === this.category.id) return false;
      if (this.category && this.isDescendantOf(item.id, this.category.id)) return false;
      return true;
    });
  }

  private buildTree(items: CategoryDto[]): CategoryDto[] {
    const map = new Map<number, CategoryDto & { children?: CategoryDto[] }>();
    const roots: CategoryDto[] = [];

    items.forEach(item => {
      map.set(item.id, { ...item, children: [] });
    });

    items.forEach(item => {
      const node = map.get(item.id)!;
      if (item.parentId && map.has(item.parentId)) {
        const parent = map.get(item.parentId)!;
        parent.children = parent.children || [];
        parent.children.push(node);
      } else {
        roots.push(node);
      }
    });

    const sortByOrder = (a: CategoryDto, b: CategoryDto) => a.sortOrder - b.sortOrder;
    roots.sort(sortByOrder);
    const sortChildren = (items: CategoryDto[]) => {
      items.forEach(item => {
        if ((item as any).children?.length) {
          (item as any).children.sort(sortByOrder);
          sortChildren((item as any).children);
        }
      });
    };
    sortChildren(roots);

    return roots;
  }

  private flattenTree(items: CategoryDto[], level: number): { id: number; name: string; level: number; indent: string }[] {
    const result: { id: number; name: string; level: number; indent: string }[] = [];

    for (const item of items) {
      const indent = level > 0 ? '　'.repeat(level) + '└ ' : '';
      result.push({
        id: item.id,
        name: item.name,
        level: level + 1,
        indent
      });

      if ((item as any).children?.length) {
        result.push(...this.flattenTree((item as any).children, level + 1));
      }
    }

    return result;
  }

  private isDescendantOf(categoryId: number, potentialAncestorId: number): boolean {
    const category = this.allCategories.find(c => c.id === categoryId);
    if (!category) return false;
    if (category.parentId === potentialAncestorId) return true;
    if (category.parentId) {
      return this.isDescendantOf(category.parentId, potentialAncestorId);
    }
    return false;
  }

  getLevelPrefix(level: number): string {
    return '　'.repeat(level - 1) + (level > 1 ? '└ ' : '');
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  onSlugInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    input.value = input.value.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '');
    this.form.get('slug')?.setValue(input.value);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.form.value;

    if (this.isEditing && this.category) {
      const request: UpdateCategoryRequest = {
        name: formValue.name,
        slug: formValue.slug,
        parentId: formValue.parentId,
        sortOrder: formValue.sortOrder,
        metaTitle: formValue.metaTitle || null,
        metaDescription: formValue.metaDescription || null,
        metaKeywords: formValue.metaKeywords || null
      };

      this.categoryService.updateCategory(this.category.id, request).subscribe({
        next: (updatedCategory) => {
          this.isSubmitting = false;
          this.onSave.emit(updatedCategory);
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Failed to update category:', err);
        }
      });
    } else {
      const request: CreateCategoryRequest = {
        name: formValue.name,
        slug: formValue.slug,
        parentId: formValue.parentId,
        sortOrder: formValue.sortOrder,
        metaTitle: formValue.metaTitle || null,
        metaDescription: formValue.metaDescription || null,
        metaKeywords: formValue.metaKeywords || null
      };

      this.categoryService.createCategory(request).subscribe({
        next: (createdCategory) => {
          this.isSubmitting = false;
          this.onSave.emit(createdCategory);
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Failed to create category:', err);
        }
      });
    }
  }
}
