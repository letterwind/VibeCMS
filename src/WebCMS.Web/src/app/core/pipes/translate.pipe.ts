import { Pipe, PipeTransform } from '@angular/core';
import { LanguageService } from '../services/language.service';

/**
 * 翻譯管道 - 用於在模板中翻譯文字
 * 使用方式：{{ 'user.profile.edit' | translate }}
 */
@Pipe({
    name: 'translate',
    standalone: true
})
export class TranslatePipe implements PipeTransform
{
    constructor(private languageService: LanguageService) { }

    /**
     * 轉換方法
     * @param key 翻譯鍵（支持多層級，如：user.profile.edit）
     * @param params 參數對象或數組，用於替換翻譯文本中的佔位符
     * 支持兩種格式：
     * 1. 數組格式：{{ 'message.maxLength' | translate:[200] }} 對應 "最多 {0} 字元"
     * 2. 對象格式：{{ 'message.greeting' | translate:{name:'John'} }} 對應 "你好，{name}"
     */
    transform(key: string, params?: Record<string, any> | any[]): string
    {
        if (!key)
        {
            return '';
        }

        return this.languageService.getTranslation(key, params);
    }
}
