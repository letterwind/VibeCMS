import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, ValidationErrors } from '@angular/forms';

/**
 * 驗證錯誤訊息元件
 * 用於顯示表單欄位的驗證錯誤訊息
 * 
 * 使用方式:
 * <app-validation-error [control]="form.get('fieldName')" [fieldName]="'欄位名稱'"></app-validation-error>
 */
@Component({
  selector: 'app-validation-error',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './validation-error.component.html',
  styleUrl: './validation-error.component.scss'
})
export class ValidationErrorComponent {
  @Input() control: AbstractControl | null = null;
  @Input() fieldName: string = '此欄位';
  @Input() customMessages: { [key: string]: string } = {};

  /**
   * 預設的驗證錯誤訊息對照表（中文）
   */
  private readonly defaultMessages: { [key: string]: (params?: any) => string } = {
    required: () => `${this.fieldName}為必填欄位`,
    minlength: (params) => `${this.fieldName}至少需要 ${params.requiredLength} 個字元`,
    maxlength: (params) => `${this.fieldName}最多 ${params.requiredLength} 個字元`,
    min: (params) => `${this.fieldName}必須大於或等於 ${params.min}`,
    max: (params) => `${this.fieldName}必須小於或等於 ${params.max}`,
    email: () => `請輸入有效的電子郵件地址`,
    pattern: () => `${this.fieldName}格式不正確`,
    serverValidation: (params) => params,
    // 自訂驗證器的錯誤訊息
    accountFormat: () => `帳號必須為 6-12 字元，包含大寫、小寫字母及數字`,
    passwordFormat: () => `密碼必須為 6-12 字元，包含大寫、小寫字母及數字`,
    passwordContainsAccount: () => `密碼不得包含帳號內容`,
    passwordSameAsCurrent: () => `新密碼不得與目前密碼相同`
  };

  get shouldShowError(): boolean {
    return this.control !== null && 
           this.control.invalid && 
           (this.control.dirty || this.control.touched);
  }

  get errorMessage(): string {
    if (!this.control || !this.control.errors) {
      return '';
    }

    const errors: ValidationErrors = this.control.errors;
    const errorKey = Object.keys(errors)[0];
    const errorParams = errors[errorKey];

    // 優先使用自訂訊息
    if (this.customMessages[errorKey]) {
      return this.customMessages[errorKey];
    }

    // 使用預設訊息
    const messageFunc = this.defaultMessages[errorKey];
    if (messageFunc) {
      return messageFunc(errorParams);
    }

    // 如果沒有對應的訊息，返回通用錯誤
    return `${this.fieldName}驗證失敗`;
  }
}
