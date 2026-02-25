import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { LanguageService } from '../services/language.service';

/**
 * 語言路由守衛 - 從 URL 中提取語言代碼並設置全局語言
 */
export const languageGuard: CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
): boolean =>
{
    const languageService = inject(LanguageService);
    const router = inject(Router);

    // 從路由參數中獲取語言代碼
    const lang = route.params['lang'] as string;

    if (lang)
    {
        // 驗證語言代碼
        if (languageService.isValidLanguageCode(lang))
        {
            // 設置當前語言
            languageService.setCurrentLanguage(lang);
            return true;
        } else
        {
            // 無效的語言代碼，重定向到默認語言
            const defaultLang = languageService.getDefaultLanguage();
            const pathSegments = getPathSegments(state.url);
            router.navigate([`/${defaultLang}`, ...pathSegments]);
            return false;
        }
    }

    // 沒有語言參數，使用檢測到的語言
    const detectedLang = languageService.detectLanguage();
    languageService.setCurrentLanguage(detectedLang);
    return true;
};

/**
 * 從完整 URL 提取路徑段
 */
function getPathSegments(url: string): string[]
{
    return url.split('/').filter(segment => segment && segment !== '');
}
