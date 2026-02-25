import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Language } from '../../../core/models/language.model';
import { LanguageService } from '../../../core/services/language.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

/**
 * 翻譯複製對話框組件
 * 用於複製實體在不同語言之間的翻譯
 */
@Component({
    selector: 'app-copy-translation-dialog',
    standalone: true,
    imports: [CommonModule, FormsModule, TranslatePipe],
    template: `
    <!-- 模態背景 -->
    <div class="modal-backdrop" *ngIf="isOpen" (click)="close()"></div>

    <!-- 模態對話框 -->
    <div class="modal" [ngClass]="{ 'modal-open': isOpen }" *ngIf="isOpen">
      <div class="modal-dialog modal-sm" (click)="$event.stopPropagation()">
        <div class="modal-content">
          <!-- 頭部 -->
          <div class="modal-header">
            <h5 class="modal-title">{{ 'translation.copyTranslation' | translate }}</h5>
            <button type="button" class="btn-close" (click)="close()"></button>
          </div>

          <!-- 內容 -->
          <div class="modal-body">
            <!-- 源語言選擇 -->
            <div class="mb-3">
              <label class="form-label">{{ 'translation.selectSourceLanguage' | translate }}</label>
              <select class="form-select" [(ngModel)]="sourceLanguage">
                <option [value]="null">--</option>
                <option *ngFor="let lang of languages$ | async" [value]="lang.languageCode">
                  {{ lang.languageName }}
                </option>
              </select>
            </div>

            <!-- 目標語言選擇 -->
            <div class="mb-3">
              <label class="form-label">{{ 'translation.selectTargetLanguage' | translate }}</label>
              <select class="form-select" [(ngModel)]="targetLanguage">
                <option [value]="null">--</option>
                <option *ngFor="let lang of languages$ | async" [value]="lang.languageCode">
                  {{ lang.languageName }}
                </option>
              </select>
            </div>

            <!-- 提示信息 -->
            <div *ngIf="sourceLanguage && targetLanguage && sourceLanguage === targetLanguage" class="alert alert-warning" role="alert">
              {{ 'translation.warningSourceTargetLanguage' | translate }}
            </div>

            <!-- 錯誤信息 -->
            <div *ngIf="errorMessage" class="alert alert-danger" role="alert">
              {{ errorMessage }}
            </div>

            <!-- 成功信息 -->
            <div *ngIf="successMessage" class="alert alert-success" role="alert">
              {{ successMessage }}
            </div>
          </div>

          <!-- 頁尾 -->
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="close()">
              {{ 'common.cancel' | translate }}
            </button>
            <button type="button" class="btn btn-primary" (click)="doCopy()" 
                    [disabled]="isLoading || !sourceLanguage || !targetLanguage || sourceLanguage === targetLanguage">
              <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
              {{ 'common.confirm' | translate }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .modal {
      display: none;
      position: fixed;
      z-index: 1050;
      left: 0;
      top: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(0, 0, 0, 0.5);
    }

    .modal.modal-open {
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .modal-backdrop {
      position: fixed;
      top: 0;
      left: 0;
      z-index: 1040;
      width: 100%;
      height: 100%;
      background-color: rgba(0, 0, 0, 0.5);
    }

    .modal-dialog {
      position: relative;
      width: 100%;
      margin: 1.75rem auto;
    }

    .modal-dialog.modal-sm {
      max-width: 400px;
    }

    .modal-content {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      pointer-events: auto;
      background-color: #fff;
      background-clip: padding-box;
      border: 1px solid rgba(0, 0, 0, 0.15);
      border-radius: 0.25rem;
      outline: 0;
    }

    .modal-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1rem;
      border-bottom: 1px solid #dee2e6;
    }

    .modal-body {
      position: relative;
      flex: 1 1 auto;
      padding: 1rem;
    }

    .modal-footer {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      padding: 1rem;
      border-top: 1px solid #dee2e6;
      gap: 0.5rem;
    }
  `]
})
export class CopyTranslationDialogComponent implements OnInit
{
    @Input() isOpen = false;
    @Output() isOpenChange = new EventEmitter<boolean>();
    @Output() copy = new EventEmitter<{ sourceLanguage: string; targetLanguage: string }>();

    languages$: Observable<Language[]>;

    sourceLanguage: string | null = null;
    targetLanguage: string | null = null;
    isLoading = false;
    errorMessage = '';
    successMessage = '';

    constructor(private languageService: LanguageService)
    {
        this.languages$ = this.languageService.getLanguages();
    }

    ngOnInit(): void
    {
        const currentLanguage = this.languageService.getCurrentLanguageSync();
        this.sourceLanguage = currentLanguage;
    }

    /**
     * 執行複製操作
     */
    doCopy(): void
    {
        if (!this.sourceLanguage || !this.targetLanguage)
        {
            this.errorMessage = '請選擇源語言和目標語言';
            return;
        }

        if (this.sourceLanguage === this.targetLanguage)
        {
            this.errorMessage = '源語言和目標語言不能相同';
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';
        this.successMessage = '';

        this.copy.emit({
            sourceLanguage: this.sourceLanguage,
            targetLanguage: this.targetLanguage
        });
    }

    /**
     * 關閉對話框
     */
    close(): void
    {
        this.isOpen = false;
        this.isOpenChange.emit(false);
        this.resetForm();
    }

    /**
     * 重置表單
     */
    private resetForm(): void
    {
        this.sourceLanguage = this.languageService.getCurrentLanguageSync();
        this.targetLanguage = null;
        this.errorMessage = '';
        this.successMessage = '';
        this.isLoading = false;
    }

    /**
     * 設置成功消息
     */
    showSuccess(message: string): void
    {
        this.successMessage = message;
        this.isLoading = false;
    }

    /**
     * 設置錯誤消息
     */
    showError(message: string): void
    {
        this.errorMessage = message;
        this.isLoading = false;
    }
}
