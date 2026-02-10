import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * 自訂驗證器集合
 * 用於前端表單驗證
 */
export class CustomValidators {
  /**
   * 帳號格式驗證
   * 規則: 6-12 字元，必須包含至少 1 個大寫字母、1 個小寫字母、1 個數字
   * 驗證: 需求 4.2
   */
  static accountFormat(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null; // 讓 required 驗證器處理空值
      }

      const errors: string[] = [];

      // 長度檢查
      if (value.length < 6 || value.length > 12) {
        errors.push('長度必須為 6-12 字元');
      }

      // 大寫字母檢查
      if (!/[A-Z]/.test(value)) {
        errors.push('必須包含至少 1 個大寫字母');
      }

      // 小寫字母檢查
      if (!/[a-z]/.test(value)) {
        errors.push('必須包含至少 1 個小寫字母');
      }

      // 數字檢查
      if (!/[0-9]/.test(value)) {
        errors.push('必須包含至少 1 個數字');
      }

      if (errors.length > 0) {
        return { accountFormat: errors.join('、') };
      }

      return null;
    };
  }

  /**
   * 密碼格式驗證
   * 規則: 6-12 字元，必須包含至少 1 個大寫字母、1 個小寫字母、1 個數字
   * 驗證: 需求 4.3
   */
  static passwordFormat(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null; // 讓 required 驗證器處理空值
      }

      const errors: string[] = [];

      // 長度檢查
      if (value.length < 6 || value.length > 12) {
        errors.push('長度必須為 6-12 字元');
      }

      // 大寫字母檢查
      if (!/[A-Z]/.test(value)) {
        errors.push('必須包含至少 1 個大寫字母');
      }

      // 小寫字母檢查
      if (!/[a-z]/.test(value)) {
        errors.push('必須包含至少 1 個小寫字母');
      }

      // 數字檢查
      if (!/[0-9]/.test(value)) {
        errors.push('必須包含至少 1 個數字');
      }

      if (errors.length > 0) {
        return { passwordFormat: errors.join('、') };
      }

      return null;
    };
  }

  /**
   * 密碼不含帳號驗證
   * 規則: 密碼不得包含帳號內容
   * 驗證: 需求 4.4
   * @param accountControlName 帳號欄位的控制項名稱
   */
  static passwordNotContainsAccount(accountControlName: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const password = control.value;
      if (!password) {
        return null;
      }

      const parent = control.parent;
      if (!parent) {
        return null;
      }

      const accountControl = parent.get(accountControlName);
      if (!accountControl || !accountControl.value) {
        return null;
      }

      const account = accountControl.value;
      if (password.toLowerCase().includes(account.toLowerCase())) {
        return { passwordContainsAccount: true };
      }

      return null;
    };
  }

  /**
   * 分類名稱驗證
   * 規則: 必填，最多 20 字元
   * 驗證: 需求 6.3, 6.4
   */
  static categoryName(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return { required: true };
      }

      if (value.length > 20) {
        return { maxlength: { requiredLength: 20, actualLength: value.length } };
      }

      return null;
    };
  }

  /**
   * 文章標題驗證
   * 規則: 必填，最多 200 字元
   * 驗證: 需求 7.2, 7.3
   */
  static articleTitle(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return { required: true };
      }

      if (value.length > 200) {
        return { maxlength: { requiredLength: 200, actualLength: value.length } };
      }

      return null;
    };
  }

  /**
   * 角色名稱驗證
   * 規則: 必填，最多 50 字元
   * 驗證: 需求 2.2
   */
  static roleName(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return { required: true };
      }

      if (value.length > 50) {
        return { maxlength: { requiredLength: 50, actualLength: value.length } };
      }

      return null;
    };
  }
}

/**
 * 驗證錯誤訊息對照表（中文）
 * 用於顯示友善的錯誤訊息
 */
export const ValidationMessages = {
  // 通用訊息
  required: (fieldName: string) => `${fieldName}為必填欄位`,
  maxlength: (fieldName: string, maxLength: number) => `${fieldName}最多 ${maxLength} 字元`,
  minlength: (fieldName: string, minLength: number) => `${fieldName}至少需要 ${minLength} 個字元`,
  min: (fieldName: string, min: number) => `${fieldName}必須大於或等於 ${min}`,
  pattern: (fieldName: string) => `${fieldName}格式不正確`,

  // 帳號密碼相關
  accountFormat: '帳號必須為 6-12 字元，包含大寫、小寫字母及數字',
  passwordFormat: '密碼必須為 6-12 字元，包含大寫、小寫字母及數字',
  passwordContainsAccount: '密碼不得包含帳號內容',
  passwordSameAsCurrent: '新密碼不得與目前密碼相同',

  // 特定欄位
  account: {
    required: '帳號為必填欄位',
    format: '帳號必須為 6-12 字元，包含大寫、小寫字母及數字'
  },
  password: {
    required: '密碼為必填欄位',
    format: '密碼必須為 6-12 字元，包含大寫、小寫字母及數字',
    containsAccount: '密碼不得包含帳號內容'
  },
  displayName: {
    required: '顯示名稱為必填欄位',
    maxlength: '顯示名稱最多 100 字元'
  },
  roleName: {
    required: '角色名稱為必填欄位',
    maxlength: '角色名稱最多 50 字元'
  },
  categoryName: {
    required: '分類名稱為必填欄位',
    maxlength: '分類名稱最多 20 字元'
  },
  articleTitle: {
    required: '文章標題為必填欄位',
    maxlength: '文章標題最多 200 字元'
  },
  functionName: {
    required: '功能名稱為必填欄位',
    maxlength: '功能名稱最多 100 字元'
  },
  functionCode: {
    required: '功能代碼為必填欄位',
    maxlength: '功能代碼最多 50 字元'
  },
  captcha: {
    required: '驗證碼為必填欄位'
  },
  slug: {
    required: '網址代稱為必填欄位',
    pattern: '網址代稱僅允許英文、數字和連字號'
  },
  category: {
    required: '請選擇分類'
  },
  content: {
    required: '文章內容為必填欄位'
  },
  hierarchyLevel: {
    required: '階層等級為必填欄位',
    min: '階層等級必須大於 0'
  }
};
