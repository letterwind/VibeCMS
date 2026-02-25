/**
 * 翻譯參數使用範例
 * 
 * 這個文件展示如何在 TypeScript 組件中使用參數化翻譯
 */

import { Component } from '@angular/core';
import { LanguageService } from './core/services/language.service';

@Component({
  selector: 'app-example',
  template: `
    <!-- 範例 1: 數組參數 - 表單驗證提示 -->
    <div class="form-text">
      {{ 'validation.maxLength' | translate:[200] }}
      <!-- 輸出：最多 200 字元 -->
    </div>

    <!-- 範例 2: 數組參數 - 顯示當前字數 -->
    <div class="form-text">
      {{ 'validation.maxLengthWithCount' | translate:[200, currentLength] }}
      <!-- 輸出：最多 200 字元 (50/200) -->
    </div>

    <!-- 範例 3: 數組參數 - 範圍驗證 -->
    <div class="form-text">
      {{ 'validation.range' | translate:[minValue, maxValue] }}
      <!-- 輸出：請輸入 1 到 100 之間的數字 -->
    </div>

    <!-- 範例 4: 對象參數 - 歡迎訊息 -->
    <div class="welcome">
      {{ 'message.welcome' | translate:{username: userName, count: messageCount} }}
      <!-- 輸出：歡迎 張三，你有 5 條新訊息 -->
    </div>

    <!-- 範例 5: 對象參數 - 檔案資訊 -->
    <div class="file-info">
      {{ 'message.fileInfo' | translate:{filename: file.name, size: file.size} }}
      <!-- 輸出：檔案 document.pdf 大小為 2.5 MB -->
    </div>

    <!-- 範例 6: 在循環中使用 -->
    <div *ngFor="let article of articles">
      {{ 'article.viewCount' | translate:[article.views] }}
      <!-- 輸出：瀏覽次數：1234 次 -->
    </div>
  `
})
export class ExampleComponent {
  // 表單相關
  currentLength = 50;
  minValue = 1;
  maxValue = 100;

  // 用戶相關
  userName = '張三';
  messageCount = 5;

  // 檔案相關
  file = {
    name: 'document.pdf',
    size: 2.5
  };

  // 文章列表
  articles = [
    { id: 1, title: '文章1', views: 1234 },
    { id: 2, title: '文章2', views: 5678 }
  ];

  constructor(private languageService: LanguageService) {}

  // ========================================
  // TypeScript 中使用翻譯參數的範例
  // ========================================

  /**
   * 範例 1: 基本的數組參數使用
   */
  getMaxLengthMessage(maxLength: number): string {
    return this.languageService.getTranslation('validation.maxLength', [maxLength]);
    // 輸出：最多 200 字元
  }

  /**
   * 範例 2: 多個數組參數
   */
  getRangeMessage(min: number, max: number): string {
    return this.languageService.getTranslation('validation.range', [min, max]);
    // 輸出：請輸入 1 到 100 之間的數字
  }

  /**
   * 範例 3: 對象參數使用
   */
  getWelcomeMessage(username: string, count: number): string {
    return this.languageService.getTranslation('message.welcome', {
      username: username,
      count: count
    });
    // 輸出：歡迎 張三，你有 5 條新訊息
  }

  /**
   * 範例 4: 表單驗證錯誤訊息
   */
  getValidationError(field: string, error: any): string {
    if (error.maxlength) {
      return this.languageService.getTranslation('validation.maxLength', [error.maxlength.requiredLength]);
    }
    if (error.minlength) {
      return this.languageService.getTranslation('validation.minLength', [error.minlength.requiredLength]);
    }
    if (error.min || error.max) {
      return this.languageService.getTranslation('validation.range', [error.min, error.max]);
    }
    return '';
  }

  /**
   * 範例 5: 檔案上傳驗證
   */
  validateFileUpload(file: File, maxSizeMB: number, allowedTypes: string[]): string | null {
    const fileSizeMB = file.size / (1024 * 1024);
    
    if (fileSizeMB > maxSizeMB) {
      return this.languageService.getTranslation('validation.fileSize', [maxSizeMB]);
      // 輸出：檔案大小不能超過 10 MB
    }

    const fileExt = file.name.split('.').pop()?.toLowerCase();
    if (fileExt && !allowedTypes.includes(fileExt)) {
      return this.languageService.getTranslation('validation.fileType', [allowedTypes.join(', ')]);
      // 輸出：只允許上傳 jpg, png, pdf 格式的檔案
    }

    return null;
  }

  /**
   * 範例 6: 動態統計訊息
   */
  getArticleStats(total: number, published: number): string {
    return this.languageService.getTranslation('message.articleStats', {
      total: total,
      published: published
    });
    // 輸出：共 50 篇文章，已發布 30 篇
  }

  /**
   * 範例 7: 錯誤訊息處理
   */
  handleError(errorType: string, params: any): void {
    let message = '';

    switch (errorType) {
      case 'timeout':
        message = this.languageService.getTranslation('error.timeout', { seconds: params.seconds });
        break;
      case 'notFound':
        message = this.languageService.getTranslation('error.notFound', { resource: params.resource });
        break;
      case 'unauthorized':
        message = this.languageService.getTranslation('error.unauthorized', { action: params.action });
        break;
    }

    // 顯示錯誤訊息
    console.error(message);
  }

  /**
   * 範例 8: 在服務中使用（如 HTTP 攔截器）
   */
  formatHttpError(status: number, url: string): string {
    switch (status) {
      case 404:
        return this.languageService.getTranslation('error.notFound', { resource: url });
      case 403:
        return this.languageService.getTranslation('error.unauthorized', { action: '存取此資源' });
      case 408:
        return this.languageService.getTranslation('error.timeout', { seconds: 30 });
      default:
        return this.languageService.getTranslation('error.unknown', { code: status });
    }
  }

  /**
   * 範例 9: 複雜的表單提示
   */
  getFormHint(fieldName: string, config: any): string {
    if (config.maxLength && config.currentLength !== undefined) {
      return this.languageService.getTranslation('validation.maxLengthWithCount', [
        config.maxLength,
        config.currentLength
      ]);
      // 輸出：最多 200 字元 (50/200)
    }
    
    if (config.maxLength) {
      return this.languageService.getTranslation('validation.maxLength', [config.maxLength]);
      // 輸出：最多 200 字元
    }

    return '';
  }

  /**
   * 範例 10: 批量處理結果訊息
   */
  getBatchResultMessage(success: number, failed: number, total: number): string {
    return this.languageService.getTranslation('message.batchResult', {
      success: success,
      failed: failed,
      total: total
    });
    // 輸出：處理完成：成功 45 筆，失敗 5 筆，共 50 筆
  }
}

/**
 * 在 Angular Material 對話框中使用
 */
@Component({
  selector: 'app-confirm-dialog',
  template: `
    <h2 mat-dialog-title>{{ 'dialog.confirm' | translate }}</h2>
    <mat-dialog-content>
      {{ 'dialog.deleteConfirm' | translate:{itemName: data.itemName} }}
      <!-- 輸出：確定要刪除「文章標題」嗎？ -->
    </mat-dialog-content>
    <mat-dialog-actions>
      <button mat-button [mat-dialog-close]="false">
        {{ 'common.cancel' | translate }}
      </button>
      <button mat-button [mat-dialog-close]="true" color="warn">
        {{ 'common.delete' | translate }}
      </button>
    </mat-dialog-actions>
  `
})
export class ConfirmDialogComponent {
  constructor(
    public data: { itemName: string }
  ) {}
}

/**
 * 在表單驗證中使用
 */
@Component({
  selector: 'app-article-form',
  template: `
    <form [formGroup]="articleForm">
      <mat-form-field>
        <mat-label>{{ 'article.title' | translate }}</mat-label>
        <input matInput formControlName="title" [maxlength]="maxTitleLength">
        <mat-hint>
          {{ 'validation.maxLengthWithCount' | translate:[maxTitleLength, titleLength] }}
        </mat-hint>
        <mat-error *ngIf="articleForm.get('title')?.hasError('required')">
          {{ 'validation.required' | translate:{field: '標題'} }}
        </mat-error>
      </mat-form-field>

      <mat-form-field>
        <mat-label>{{ 'article.content' | translate }}</mat-label>
        <textarea matInput formControlName="content" [maxlength]="maxContentLength"></textarea>
        <mat-hint>
          {{ 'validation.maxLength' | translate:[maxContentLength] }}
        </mat-hint>
      </mat-form-field>
    </form>
  `
})
export class ArticleFormExampleComponent {
  maxTitleLength = 200;
  maxContentLength = 5000;

  get titleLength(): number {
    return this.articleForm.get('title')?.value?.length || 0;
  }

  articleForm: any; // FormGroup 實例
}
