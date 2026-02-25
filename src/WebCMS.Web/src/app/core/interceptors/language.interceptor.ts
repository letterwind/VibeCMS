import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LanguageService } from '../services/language.service';

/**
 * 語言 HTTP 攔截器 - 自動為所有 HTTP 請求添加語言 Header
 */
export const languageInterceptor: HttpInterceptorFn = (req, next) =>
{
    const languageService = inject(LanguageService);

    // 獲取當前語言
    const currentLang = languageService.getCurrentLanguageSync();

    // 克隆請求並添加語言 Header 和查詢參數
    const langReq = req.clone({
        setHeaders: {
            'Accept-Language': currentLang
        },
        setParams: {
            lang: currentLang
        }
    });

    return next(langReq);
};
