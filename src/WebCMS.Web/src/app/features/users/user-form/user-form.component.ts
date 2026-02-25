import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap, of, catchError } from 'rxjs';
import { UserService } from '../../../core/services/user.service';
import { LanguageService } from '../../../core/services/language.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { UserDto, CreateUserRequest, UpdateUserRequest } from '../../../core/models/user.model';
import { RoleDto } from '../../../core/models/role.model';
import { ValidationErrorComponent } from '../../../shared/components/validation-error/validation-error.component';
import { CustomValidators } from '../../../shared/validators/custom-validators';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ValidationErrorComponent, TranslatePipe],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent implements OnInit, OnChanges {
  @Input() user: UserDto | null = null;
  @Input() roles: RoleDto[] = [];
  @Input() isEditing = false;
  @Output() onSave = new EventEmitter<UserDto>();
  @Output() onCancel = new EventEmitter<void>();

  form!: FormGroup;
  isSubmitting = false;
  errorMessage = '';
  selectedRoleIds: number[] = [];

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    public languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.setupValidation();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['user'] && this.form) {
      this.updateForm();
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      account: ['', [Validators.required, CustomValidators.accountFormat()]],
      password: ['', this.isEditing ? [] : [Validators.required, CustomValidators.passwordFormat(), CustomValidators.passwordNotContainsAccount('account')]],
      displayName: ['', [Validators.required, Validators.maxLength(100)]]
    });
    this.updateForm();
  }

  private setupValidation(): void {
    this.form.get('account')?.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(value => {
        if (!value || this.isEditing) return of(null);
        return this.userService.validateAccount(value).pipe(
          catchError(() => of(null))
        );
      })
    ).subscribe(result => {
      if (result && !result.isValid) {
        this.form.get('account')?.setErrors({ serverValidation: result.errors.join(', ') });
      }
    });

    this.form.get('password')?.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(value => {
        if (!value) return of(null);
        const account = this.form.get('account')?.value || '';
        return this.userService.validatePassword(value, account).pipe(
          catchError(() => of(null))
        );
      })
    ).subscribe(result => {
      if (result && !result.isValid) {
        this.form.get('password')?.setErrors({ serverValidation: result.errors.join(', ') });
      }
    });
  }

  private updateForm(): void {
    if (this.user) {
      this.form.patchValue({
        account: this.user.account,
        password: '',
        displayName: this.user.displayName
      });
      this.selectedRoleIds = this.user.roles.map(r => r.id);
      
      this.form.get('password')?.clearValidators();
      this.form.get('password')?.setValidators([CustomValidators.passwordFormat(), CustomValidators.passwordNotContainsAccount('account')]);
      this.form.get('password')?.updateValueAndValidity();
    } else {
      this.form.reset({
        account: '',
        password: '',
        displayName: ''
      });
      this.selectedRoleIds = [];
      
      this.form.get('password')?.setValidators([Validators.required, CustomValidators.passwordFormat(), CustomValidators.passwordNotContainsAccount('account')]);
      this.form.get('password')?.updateValueAndValidity();
    }
    this.errorMessage = '';
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  isFieldValid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return field ? field.valid && field.dirty && field.value : false;
  }

  isRoleSelected(roleId: number): boolean {
    return this.selectedRoleIds.includes(roleId);
  }

  toggleRole(roleId: number): void {
    const index = this.selectedRoleIds.indexOf(roleId);
    if (index > -1) {
      this.selectedRoleIds.splice(index, 1);
    } else {
      this.selectedRoleIds.push(roleId);
    }
  }

  onSubmit(): void {
    this.errorMessage = '';

    if (!this.isEditing && this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.form.value;

    if (this.isEditing && this.user) {
      const request: UpdateUserRequest = {
        displayName: formValue.displayName,
        newPassword: formValue.password || null,
        roleIds: this.selectedRoleIds.length > 0 ? this.selectedRoleIds : null
      };

      this.userService.updateUser(this.user.id, request).subscribe({
        next: (updatedUser) => {
          this.isSubmitting = false;
          this.onSave.emit(updatedUser);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.errorMessage = err.error?.message || this.languageService.getTranslation('user.updateUserFailed');
        }
      });
    } else {
      const request: CreateUserRequest = {
        account: formValue.account,
        password: formValue.password,
        displayName: formValue.displayName,
        roleIds: this.selectedRoleIds.length > 0 ? this.selectedRoleIds : null
      };

      this.userService.createUser(request).subscribe({
        next: (createdUser) => {
          this.isSubmitting = false;
          this.onSave.emit(createdUser);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.errorMessage = err.error?.message || this.languageService.getTranslation('user.createUserFailed');
        }
      });
    }
  }
}
