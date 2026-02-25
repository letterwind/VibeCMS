import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { Language } from '../../../core/models/language.model';

/**
 * 語言選擇器組件 - 在 UI 中顯示語言選擇下拉菜單
 */
@Component({
    selector: 'app-language-selector',
    standalone: true,
    imports: [CommonModule, RouterModule],
    template: `
    <div class="language-selector">
      <select class="form-select form-select-sm" (change)="onLanguageChange($event)">
        <option *ngFor="let lang of languages$ | async" 
                [value]="lang.languageCode"
                [selected]="lang.languageCode === (currentLanguage$ | async)">
          {{ lang.languageName }}
        </option>
      </select>
    </div>
  `,
    styles: [`
    .language-selector {
      margin: 0;
      padding: 0;
    }

    .form-select {
      min-width: 120px;
      font-size: 0.875rem;
    }
  `]
})
export class LanguageSelectorComponent implements OnInit
{
    languages$: Observable<Language[]>;
    currentLanguage$: Observable<string>;

    constructor(private languageService: LanguageService)
    {
        this.languages$ = this.languageService.getLanguages();
        this.currentLanguage$ = this.languageService.getCurrentLanguage();
    }

    ngOnInit(): void
    {
        // 語言列表已在服務中自動加載
    }

    /**
     * 處理語言選擇變化
     */
    onLanguageChange(event: Event): void
    {
        const select = event.target as HTMLSelectElement;
        const selectedLanguage = select.value;

        if (selectedLanguage)
        {
            this.languageService.setCurrentLanguage(selectedLanguage);
            // 可選：導航到新語言的路由
            // this.router.navigate([selectedLanguage, 'admin']);
        }
    }
}
