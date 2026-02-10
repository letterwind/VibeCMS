import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RoleService } from '../../../core/services/role.service';
import { RoleDto, CreateRoleRequest, UpdateRoleRequest } from '../../../core/models/role.model';
import { ValidationErrorComponent } from '../../../shared/components/validation-error/validation-error.component';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ValidationErrorComponent],
  templateUrl: './role-form.component.html',
  styleUrl: './role-form.component.scss'
})
export class RoleFormComponent implements OnInit, OnChanges {
  @Input() role: RoleDto | null = null;
  @Input() isEditing = false;
  @Output() onSave = new EventEmitter<RoleDto>();
  @Output() onCancel = new EventEmitter<void>();

  form!: FormGroup;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['role'] && this.form) {
      this.updateForm();
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', [Validators.maxLength(200)]],
      hierarchyLevel: [1, [Validators.required, Validators.min(1)]]
    });
    this.updateForm();
  }

  private updateForm(): void {
    if (this.role) {
      this.form.patchValue({
        name: this.role.name,
        description: this.role.description || '',
        hierarchyLevel: this.role.hierarchyLevel
      });
    } else {
      this.form.reset({
        name: '',
        description: '',
        hierarchyLevel: 1
      });
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.form.value;

    if (this.isEditing && this.role) {
      const request: UpdateRoleRequest = {
        name: formValue.name,
        description: formValue.description || null,
        hierarchyLevel: formValue.hierarchyLevel
      };

      this.roleService.updateRole(this.role.id, request).subscribe({
        next: (updatedRole) => {
          this.isSubmitting = false;
          this.onSave.emit(updatedRole);
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Failed to update role:', err);
        }
      });
    } else {
      const request: CreateRoleRequest = {
        name: formValue.name,
        description: formValue.description || null,
        hierarchyLevel: formValue.hierarchyLevel
      };

      this.roleService.createRole(request).subscribe({
        next: (createdRole) => {
          this.isSubmitting = false;
          this.onSave.emit(createdRole);
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Failed to create role:', err);
        }
      });
    }
  }
}
