import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';
import { Language } from '../models/language.model';
import { LanguageFileExport } from '../models/language-resource.model';
import { environment } from '../../../environments/environment';

/**
 * 語言管理服務
 */
@Injectable({
    providedIn: 'root'
})
export class LanguageService
{
    // 可用語言列表
    private languages$ = new BehaviorSubject<Language[]>([]);

    // 當前活躍語言
    private currentLanguage$ = new BehaviorSubject<string>('zh-TW');

    // 當前語言的資源字典
    private languageResources$ = new BehaviorSubject<Record<string, any>>({});

    // API 端點
    // private apiUrl = `${environment.apiUrl}/api/languageResource`;
    private languageResourceUrl = `${environment.apiUrl}/api/languageResource`;
    private languageFileUrl = `${environment.apiUrl}/api/languageFile`;

    // 默認語言
    private readonly DEFAULT_LANGUAGE = 'zh-TW';

    // 支持的語言代碼
    private readonly SUPPORTED_LANGUAGES = ['zh-TW', 'en-US', 'ja-JP'];

    // 語言代碼到語言名稱的映射
    private readonly LANGUAGE_NAMES: Record<string, string> = {
        'zh-TW': '繁體中文',
        'en-US': 'English',
        'ja-JP': '日本語'
    };

    // 快取資源載入狀態
    private resourcesLoaded: Map<string, boolean> = new Map();

    constructor(private http: HttpClient)
    {
        this.initializeLanguages();
        // 加載初始語言資源
        this.loadPreferredLanguage();
        // 在當前語言變化時加載資源
        this.currentLanguage$.subscribe(languageCode =>
        {
            this.loadLanguageResources(languageCode).catch(error =>
            {
                console.error(`Failed to load language resources for ${languageCode}:`, error);
            });
        });
    }

    /**
     * 初始化語言列表
     */
    private initializeLanguages(): void
    {
        this.http.get<Language[]>(this.languageResourceUrl).subscribe({
            next: (languages) =>
            {
                this.languages$.next(languages);
            },
            error: () =>
            {
                // 如果載入失敗，使用本地硬編碼的語言列表
                this.languages$.next(this.getDefaultLanguages());
            }
        });
    }

    /**
     * 獲取默認語言列表
     */
    private getDefaultLanguages(): Language[]
    {
        return [
            { id: 1, languageCode: 'zh-TW', languageName: '繁體中文', isActive: true, sortOrder: 1 },
            { id: 2, languageCode: 'en-US', languageName: 'English', isActive: true, sortOrder: 2 },
            { id: 3, languageCode: 'ja-JP', languageName: '日本語', isActive: true, sortOrder: 3 }
        ];
    }

    /**
     * 取得可用語言列表
     */
    getLanguages(): Observable<Language[]>
    {
        return this.languages$.asObservable();
    }

    /**
     * 取得當前活躍語言
     */
    getCurrentLanguage(): Observable<string>
    {
        return this.currentLanguage$.asObservable();
    }

    /**
     * 取得當前活躍語言（同步）
     */
    getCurrentLanguageSync(): string
    {
        return this.currentLanguage$.value;
    }

    /**
     * 設置當前語言
     */
    setCurrentLanguage(languageCode: string): void
    {
        if (this.isValidLanguageCode(languageCode))
        {
            this.currentLanguage$.next(languageCode);
            // 保存到 localStorage
            localStorage.setItem('preferredLanguage', languageCode);
            // 異步加載相應語言的資源
            this.loadLanguageResources(languageCode).catch(error =>
            {
                console.error(`Failed to load resources for language ${languageCode}:`, error);
            });
        }
    }

    /**
     * 從 localStorage 加載 preferredLanguage
     */
    loadPreferredLanguage(): void
    {
        const saved = localStorage.getItem('preferredLanguage');
        if (saved && this.isValidLanguageCode(saved))
        {
            this.currentLanguage$.next(saved);
        }
    }

    /**
     * 驗證語言代碼是否有效
     */
    isValidLanguageCode(code: string): boolean
    {
        return this.SUPPORTED_LANGUAGES.includes(code);
    }

    /**
     * 取得語言名稱
     */
    getLanguageName(code: string): string
    {
        return this.LANGUAGE_NAMES[code] || code;
    }

    /**
     * 取得默認語言
     */
    getDefaultLanguage(): string
    {
        return this.DEFAULT_LANGUAGE;
    }

    /**
     * 從 URL 或 HTTP Header 檢測語言
     */
    detectLanguage(urlLang?: string): string
    {
        // 優先使用 URL 參數
        if (urlLang && this.isValidLanguageCode(urlLang))
        {
            return urlLang;
        }

        // 其次使用已保存的首選語言
        const saved = localStorage.getItem('preferredLanguage');
        if (saved && this.isValidLanguageCode(saved))
        {
            return saved;
        }

        // 嘗試從瀏覽器語言偵測
        const browserLang = this.getBrowserLanguage();
        if (browserLang && this.isValidLanguageCode(browserLang))
        {
            return browserLang;
        }

        return this.DEFAULT_LANGUAGE;
    }

    /**
     * 獲取瀏覽器語言
     */
    private getBrowserLanguage(): string | null
    {
        const browserLang = navigator.language || (navigator as any).userLanguage;
        if (!browserLang)
        {
            return null;
        }

        // 規範化格式 (e.g., zh-TW)
        return browserLang.replace('_', '-').toUpperCase() === 'ZH-TW' ? 'zh-TW' :
            browserLang.replace('_', '-').toUpperCase() === 'EN-US' ? 'en-US' :
                browserLang.replace('_', '-').toUpperCase() === 'JA-JP' ? 'ja-JP' :
                    null;
    }

    /**
     * 取得所有支持的語言代碼
     */
    getSupportedLanguageCodes(): string[]
    {
        return [...this.SUPPORTED_LANGUAGES];
    }

    /**
     * 載入語言資源（從 API 或靜態檔案）
     */
    async loadLanguageResources(languageCode: string): Promise<void>
    {
        // 如果已經載入過，跳過
        if (this.resourcesLoaded.get(languageCode))
        {
            return;
        }

        try
        {
            // 優先嘗試從 API 加載
            const resources = await this.http.get<Record<string, any>>(
                `${this.languageFileUrl}/${languageCode}.json`
            ).toPromise();

            if (resources)
            {
                this.languageResources$.next(resources);
                this.resourcesLoaded.set(languageCode, true);
                return;
            }
        }
        catch (error)
        {
            console.warn(`Failed to load language resources from API for ${languageCode}`, error);
        }

        try
        {
            // 回退到靜態資源
            const resources = await this.http.get<Record<string, any>>(
                `/assets/i18n/${languageCode}.json`
            ).toPromise();

            if (resources)
            {
                this.languageResources$.next(resources);
                this.resourcesLoaded.set(languageCode, true);
            }
        }
        catch (error)
        {
            console.error(`Failed to load language resources for ${languageCode}`, error);
        }
    }

    /**
     * 取得語言資源
     */
    getLanguageResources(): Observable<Record<string, any>>
    {
        return this.languageResources$.asObservable();
    }

    /**
     * 取得語言資源（同步）
     */
    getLanguageResourcesSync(): Record<string, any>
    {
        return this.languageResources$.value;
    }

    /**
     * 取得翻譯文字 - 支持多層級鍵（如：user.profile.edit）
     * @param key 翻譯鍵
     * @param params 參數對象或數組，用於替換翻譯文本中的佔位符
     * 支持兩種格式：
     * 1. 數組格式：['value1', 'value2'] 對應 {0}, {1}
     * 2. 對象格式：{name: 'John', count: 5} 對應 {name}, {count}
     */
    getTranslation(key: string, params?: Record<string, any> | any[]): string
    {
        const keys = key.split('.');
        let value: any = this.languageResources$.value;

        for (const k of keys)
        {
            if (value && typeof value === 'object' && k in value)
            {
                value = value[k];
            }
            else
            {
                return key; // 如果未找到，返回原鍵
            }
        }

        let translation = typeof value === 'string' ? value : key;

        // 如果有參數，進行替換
        if (params)
        {
            translation = this.replaceParams(translation, params);
        }

        return translation;
    }

    /**
     * 替換翻譯文本中的參數佔位符
     * @param text 原始文本
     * @param params 參數對象或數組
     */
    private replaceParams(text: string, params: Record<string, any> | any[]): string
    {
        if (Array.isArray(params))
        {
            // 數組格式：替換 {0}, {1}, {2} 等
            return text.replace(/\{(\d+)\}/g, (match, index) =>
            {
                const paramIndex = parseInt(index, 10);
                return paramIndex < params.length ? String(params[paramIndex]) : match;
            });
        }
        else
        {
            // 對象格式：替換 {key} 等
            return text.replace(/\{(\w+)\}/g, (match, key) =>
            {
                return key in params ? String(params[key]) : match;
            });
        }
    }

    /**
     * 清除語言資源快取
     */
    clearResourceCache(languageCode?: string): void
    {
        if (languageCode)
        {
            this.resourcesLoaded.delete(languageCode);
        }
        else
        {
            this.resourcesLoaded.clear();
        }
    }
}
