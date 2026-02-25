import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ValidationErrorComponent } from '../../../shared/components/validation-error/validation-error.component';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ValidationErrorComponent, TranslatePipe],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  captchaImage = signal<string>('');
  captchaToken = signal<string>('');
  errorMessage = signal<string>('');
  isLoading = signal<boolean>(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      account: ['', Validators.required],
      password: ['', Validators.required],
      captcha: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.refreshCaptcha();
  }

  refreshCaptcha(): void {
    this.authService.getCaptcha().subscribe({
      next: (response) => {
        this.captchaImage.set(response.imageBase64);
        this.captchaToken.set(response.token);
        this.loginForm.patchValue({ captcha: '' });
      },
      error: () => {
        this.errorMessage.set('無法載入驗證碼，請稍後再試');
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    const request = {
      account: this.loginForm.value.account,
      password: this.loginForm.value.password,
      captcha: this.loginForm.value.captcha,
      captchaToken: this.captchaToken()
    };

    this.authService.login(request).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.user.isPasswordExpired) {
          this.router.navigate(['/admin/change-password']);
        } else {
          this.router.navigate(['/admin/dashboard']);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || '登入失敗，請稍後再試');
        this.refreshCaptcha();
      }
    });
  }
}
