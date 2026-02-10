import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { FunctionService } from '../../../core/services/function.service';
import { FunctionDto, CreateFunctionRequest, UpdateFunctionRequest } from '../../../core/models/function.model';
import { ValidationErrorComponent } from '../../../shared/components/validation-error/validation-error.component';

const BOOTSTRAP_ICONS = [
  'bi-house', 'bi-house-fill', 'bi-gear', 'bi-gear-fill',
  'bi-person', 'bi-person-fill', 'bi-people', 'bi-people-fill',
  'bi-file-text', 'bi-file-text-fill', 'bi-folder', 'bi-folder-fill',
  'bi-list', 'bi-grid', 'bi-table', 'bi-kanban',
  'bi-bar-chart', 'bi-pie-chart', 'bi-graph-up', 'bi-graph-down',
  'bi-shield', 'bi-shield-fill', 'bi-lock', 'bi-lock-fill',
  'bi-key', 'bi-key-fill', 'bi-box', 'bi-box-fill',
  'bi-cart', 'bi-cart-fill', 'bi-bag', 'bi-bag-fill',
  'bi-credit-card', 'bi-credit-card-fill', 'bi-wallet', 'bi-wallet-fill',
  'bi-calendar', 'bi-calendar-fill', 'bi-clock', 'bi-clock-fill',
  'bi-bell', 'bi-bell-fill', 'bi-envelope', 'bi-envelope-fill',
  'bi-chat', 'bi-chat-fill', 'bi-megaphone', 'bi-megaphone-fill',
  'bi-image', 'bi-image-fill', 'bi-camera', 'bi-camera-fill',
  'bi-film', 'bi-music-note', 'bi-book', 'bi-book-fill',
  'bi-newspaper', 'bi-journal', 'bi-pencil', 'bi-pencil-fill',
  'bi-trash', 'bi-trash-fill', 'bi-archive', 'bi-archive-fill',
  'bi-download', 'bi-upload', 'bi-cloud', 'bi-cloud-fill',
  'bi-database', 'bi-server', 'bi-cpu', 'bi-hdd',
  'bi-globe', 'bi-link', 'bi-share', 'bi-bookmark',
  'bi-star', 'bi-star-fill', 'bi-heart', 'bi-heart-fill',
  'bi-flag', 'bi-flag-fill', 'bi-tag', 'bi-tag-fill',
  'bi-tools', 'bi-wrench', 'bi-hammer', 'bi-screwdriver',
  'bi-menu-button-wide', 'bi-menu-app', 'bi-layout-sidebar',
  'bi-speedometer', 'bi-speedometer2', 'bi-sliders', 'bi-toggles','bi-layout-text-window','bi-layout-text-window-reverse'
];

interface FlattenedOption {
  id: number;
  name: string;
  code: string;
  indent: string;
  level: number;
}

@Component({
  selector: 'app-function-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, ValidationErrorComponent],
  templateUrl: './function-form.component.html',
  styleUrl: './function-form.component.scss'
})
export class FunctionFormComponent implements OnInit, OnChanges {
  @Input() function: FunctionDto | null = null;
  @Input() parentFunctions: FunctionDto[] = [];
  @Input() isEditing = false;
  @Output() onSave = new EventEmitter<FunctionDto>();
  @Output() onCancel = new EventEmitter<void>();

  form!: FormGroup;
  isSubmitting = false;
  showIconPicker = false;
  iconSearchTerm = '';
  icons = BOOTSTRAP_ICONS;

  constructor(
    private fb: FormBuilder,
    private functionService: FunctionService
  ) {}

  get flattenedParentOptions(): FlattenedOption[] {
    const tree = this.buildTree(this.parentFunctions);
    const options = this.flattenTree(tree, 0);
    if (this.isEditing && this.function) {
      return options.filter(o => o.id !== this.function!.id);
    }
    return options;
  }

  get filteredIcons(): string[] {
    if (!this.iconSearchTerm) {
      return this.icons;
    }
    const term = this.iconSearchTerm.toLowerCase();
    return this.icons.filter(icon => icon.toLowerCase().includes(term));
  }

  ngOnInit(): void {
    this.initForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['function'] && this.form) {
      this.updateForm();
    }
  }

  private buildTree(items: FunctionDto[]): FunctionDto[] {
    const map = new Map<number, FunctionDto>();
    const roots: FunctionDto[] = [];

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

    return roots;
  }

  private flattenTree(items: FunctionDto[], level: number): FlattenedOption[] {
    const result: FlattenedOption[] = [];
    const indent = '　'.repeat(level);

    for (const item of items) {
      result.push({
        id: item.id,
        name: item.name,
        code: item.code,
        indent: level > 0 ? indent + '└ ' : '',
        level
      });

      if (item.children && item.children.length > 0) {
        result.push(...this.flattenTree(item.children, level + 1));
      }
    }

    return result;
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      code: ['', [Validators.required, Validators.maxLength(50)]],
      url: ['', [Validators.maxLength(500)]],
      openInNewWindow: [false],
      icon: [''],
      parentId: [null],
      sortOrder: [0]
    });
    this.updateForm();
  }

  private updateForm(): void {
    if (this.function) {
      this.form.patchValue({
        name: this.function.name,
        code: this.function.code,
        url: this.function.url || '',
        openInNewWindow: this.function.openInNewWindow,
        icon: this.function.icon || '',
        parentId: this.function.parentId,
        sortOrder: this.function.sortOrder
      });
    } else {
      this.form.reset({
        name: '',
        code: '',
        url: '',
        openInNewWindow: false,
        icon: '',
        parentId: null,
        sortOrder: 0
      });
    }
    this.showIconPicker = false;
    this.iconSearchTerm = '';
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  toggleIconPicker(): void {
    this.showIconPicker = !this.showIconPicker;
  }

  selectIcon(icon: string): void {
    this.form.patchValue({ icon });
    this.showIconPicker = false;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.form.value;

    if (this.isEditing && this.function) {
      const request: UpdateFunctionRequest = {
        name: formValue.name,
        code: formValue.code,
        url: formValue.url || null,
        openInNewWindow: formValue.openInNewWindow,
        icon: formValue.icon || null,
        parentId: formValue.parentId,
        sortOrder: formValue.sortOrder
      };

      this.functionService.updateFunction(this.function.id, request).subscribe({
        next: (updatedFunction) => {
          this.isSubmitting = false;
          this.onSave.emit(updatedFunction);
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Failed to update function:', err);
        }
      });
    } else {
      const request: CreateFunctionRequest = {
        name: formValue.name,
        code: formValue.code,
        url: formValue.url || null,
        openInNewWindow: formValue.openInNewWindow,
        icon: formValue.icon || null,
        parentId: formValue.parentId,
        sortOrder: formValue.sortOrder
      };

      this.functionService.createFunction(request).subscribe({
        next: (createdFunction) => {
          this.isSubmitting = false;
          this.onSave.emit(createdFunction);
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Failed to create function:', err);
        }
      });
    }
  }
}
