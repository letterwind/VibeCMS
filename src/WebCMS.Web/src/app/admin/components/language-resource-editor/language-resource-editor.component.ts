import { TranslatePipe } from './../../../core/pipes/translate.pipe';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable, BehaviorSubject } from 'rxjs';
import { Language } from '../../../core/models/language.model';
import { LanguageService } from '../../../core/services/language.service';
import { LanguageResourceService } from '../../../core/services/language-resource.service';
import
{
    LanguageResource,
    CreateOrUpdateLanguageResourceRequest
} from '../../../core/models/language-resource.model';

/**
 * 語言資源編輯器組件 - 管理系統 UI 文字翻譯
 */
@Component({
    selector: 'app-language-resource-editor',
    standalone: true,
    imports: [CommonModule, FormsModule, TranslatePipe],
    template: `
    <div class="language-resource-editor card">
      <div class="card-header">
        <h5>{{ 'common.languageResourceEditor' | translate }}</h5>
      </div>

      <div class="card-body">
        <!-- 語言選擇 -->
        <div class="mb-3">
          <label class="form-label">{{ 'common.language' | translate }}</label>
          <select class="form-select" [(ngModel)]="selectedLanguage" (change)="onLanguageChange()">
            <option *ngFor="let lang of languages$ | async" [value]="lang.languageCode">
              {{ lang.languageName }}
            </option>
          </select>
        </div>

        <!-- 編輯模式選擇 -->
        <div class="mb-3">
          <div class="btn-group" role="group">
            <input type="radio" class="btn-check" id="modeForm" value="form" [(ngModel)]="editMode">
            <label class="btn btn-outline-primary" for="modeForm">{{ 'common.formEditor' | translate }}</label>

            <input type="radio" class="btn-check" id="modeJson" value="json" [(ngModel)]="editMode">
            <label class="btn btn-outline-primary" for="modeJson">{{ 'common.jsonEditor' | translate }}</label>
          </div>
        </div>

        <!-- 表格編輯模式 -->
        <div *ngIf="editMode === 'form'" class="form-editor">
          <div class="table-responsive">
            <table class="table table-sm table-striped">
              <thead>
                <tr>
                  <th>{{ 'common.key' | translate }}</th>
                  <th>{{ 'common.value' | translate }}</th>
                  <th style="width: 120px;">{{ 'common.actions' | translate }}</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let resource of resources$ | async">
                  <td>
                    <small>{{ resource.resourceKey }}</small>
                  </td>
                  <td>
                    <input type="text" class="form-control form-control-sm" [(ngModel)]="resource.resourceValue">
                  </td>
                  <td>
                    <button class="btn btn-primary btn-sm" (click)="updateResource(resource)">
                      {{ 'common.save' | translate }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 新增資源 -->
          <div class="mt-3 p-3 border rounded">
            <h6>{{ 'common.addNewResource' | translate }}</h6>
            <div class="row">
              <div class="col-md-6">
                <label class="form-label">{{ 'common.key' | translate }}</label>
                <input type="text" class="form-control" [(ngModel)]="newResourceKey" placeholder="user.profile.edit">
              </div>
              <div class="col-md-6">
                <label class="form-label">{{ 'common.value' | translate }}</label>
                <input type="text" class="form-control" [(ngModel)]="newResourceValue" placeholder="編輯個人資料">
              </div>
            </div>
            <button class="btn btn-success mt-2" (click)="addNewResource()">
              {{ 'common.add' | translate }}
            </button>
          </div>
        </div>

        <!-- JSON 編輯模式 -->
        <div *ngIf="editMode === 'json'" class="json-editor">
          <div class="mb-3">
            <label class="form-label">{{ 'common.jsonContent' | translate }}</label>
            <textarea class="form-control" rows="15" [(ngModel)]="jsonContent" style="font-family: monospace;"></textarea>
          </div>
          <div class="button-group">
            <button class="btn btn-primary" (click)="importJson()">
              {{ 'common.import' | translate }}
            </button>
            <button class="btn btn-secondary" (click)="exportJson()">
              {{ 'common.export' | translate }}
            </button>
          </div>
        </div>

        <!-- 批量操作 -->
        <div class="mt-4 pt-3 border-top">
          <h6>{{ 'common.batchOperations' | translate }}</h6>
          <button class="btn btn-warning" (click)="clearCache()">
            {{ 'common.clearCache' | translate }}
          </button>
          <button class="btn btn-info" (click)="refreshResources()">
            {{ 'common.refresh' | translate }}
          </button>
        </div>

        <!-- 消息提示 -->
        <div *ngIf="message" [ngClass]="'mt-3 alert alert-' + messageType" role="alert">
          {{ message }}
        </div>
      </div>
    </div>
  `,
    styles: [`
    .language-resource-editor {
      max-width: 900px;
    }

    .table-responsive {
      max-height: 500px;
      overflow-y: auto;
    }

    .form-editor, .json-editor {
      margin-top: 20px;
    }

    .button-group button {
      margin-right: 5px;
    }

    .btn-group {
      margin-bottom: 15px;
    }
  `]
})
export class LanguageResourceEditorComponent implements OnInit
{
    languages$: Observable<Language[]>;
    resources$ = new BehaviorSubject<LanguageResource[]>([]);

    selectedLanguage = 'zh-TW';
    editMode: 'form' | 'json' = 'form';
    jsonContent = '';
    newResourceKey = '';
    newResourceValue = '';

    message = '';
    messageType = 'info'; // info, success, danger

    constructor(
        private languageService: LanguageService,
        private languageResourceService: LanguageResourceService
    )
    {
        this.languages$ = this.languageService.getLanguages();
    }

    ngOnInit(): void
    {
        this.selectedLanguage = this.languageService.getCurrentLanguageSync();
        this.loadResources();
    }

    /**
     * 載入語言資源
     */
    loadResources(): void
    {
        this.languageResourceService.getResourcesByLanguage(this.selectedLanguage).subscribe({
            next: (response) =>
            {
                if (response.success && response.data)
                {
                    this.resources$.next(response.data);
                    this.showMessage('資源載入成功', 'success');
                }
            },
            error: (error) =>
            {
                this.showMessage('資源載入失敗: ' + error.message, 'danger');
            }
        });
    }

    /**
     * 語言變化事件
     */
    onLanguageChange(): void
    {
        this.languageService.setCurrentLanguage(this.selectedLanguage);
        this.loadResources();
    }

    /**
     * 更新單個資源
     */
    updateResource(resource: LanguageResource): void
    {
        const request: CreateOrUpdateLanguageResourceRequest = {
            languageCode: this.selectedLanguage,
            resourceKey: resource.resourceKey,
            resourceValue: resource.resourceValue,
            resourceType: 'UI'
        };

        this.languageResourceService.updateResource(resource.id, request).subscribe({
            next: () =>
            {
                this.showMessage('資源已更新', 'success');
            },
            error: (error) =>
            {
                this.showMessage('更新失敗: ' + error.message, 'danger');
            }
        });
    }

    /**
     * 新增資源
     */
    addNewResource(): void
    {
        if (!this.newResourceKey || !this.newResourceValue)
        {
            this.showMessage('請填寫鍵和值', 'danger');
            return;
        }

        const request: CreateOrUpdateLanguageResourceRequest = {
            languageCode: this.selectedLanguage,
            resourceKey: this.newResourceKey,
            resourceValue: this.newResourceValue,
            resourceType: 'UI'
        };

        this.languageResourceService.createResource(request).subscribe({
            next: () =>
            {
                this.showMessage('資源已新增', 'success');
                this.newResourceKey = '';
                this.newResourceValue = '';
                this.loadResources();
            },
            error: (error) =>
            {
                this.showMessage('新增失敗: ' + error.message, 'danger');
            }
        });
    }

    /**
     * 匯入 JSON
     */
    importJson(): void
    {
        try
        {
            const fileContent = JSON.parse(this.jsonContent);

            this.languageResourceService.importResources({
                languageCode: this.selectedLanguage,
                fileContent,
                overwrite: true
            }).subscribe({
                next: () =>
                {
                    this.showMessage('資源已匯入', 'success');
                    this.loadResources();
                },
                error: (error) =>
                {
                    this.showMessage('匯入失敗: ' + error.message, 'danger');
                }
            });
        }
        catch (error)
        {
            this.showMessage('JSON 格式錯誤', 'danger');
        }
    }

    /**
     * 匯出 JSON
     */
    exportJson(): void
    {
        this.languageResourceService.exportResources(this.selectedLanguage).subscribe({
            next: (response) =>
            {
                this.jsonContent = JSON.stringify(response, null, 2);
                this.showMessage('資源已匯出', 'success');
            },
            error: (error) =>
            {
                this.showMessage('匯出失敗: ' + error.message, 'danger');
            }
        });
    }

    /**
     * 清除快取
     */
    clearCache(): void
    {
        this.languageService.clearResourceCache();
        this.showMessage('快取已清除', 'info');
    }

    /**
     * 刷新資源
     */
    refreshResources(): void
    {
        this.loadResources();
    }

    /**
     * 顯示消息
     */
    private showMessage(message: string, type: 'info' | 'success' | 'danger'): void
    {
        this.message = message;
        this.messageType = type;
        setTimeout(() =>
        {
            this.message = '';
        }, 3000);
    }
}
