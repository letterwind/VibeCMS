import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../../shared/components/toast/notification.service';

/**
 * HTTP 錯誤攔截器
 * 處理 401、403、404、422、500 等 HTTP 錯誤
 * 
 * 驗證: 需求 1.6, 3.3
 * - 1.6: IF 帳號或密碼錯誤 THEN THE CMS_System SHALL 顯示錯誤訊息但不透露具體錯誤原因
 * - 3.3: WHEN 使用者嘗試存取無權限的功能 THEN THE CMS_System SHALL 拒絕存取並顯示權限不足訊息
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // 不處理登入請求的錯誤，讓登入頁面自行處理
      const isLoginRequest = req.url.includes('/login');
      switch (error.status) {
        case 401:
          // 未授權 - 清除認證並導向登入頁
          // 需求 1.6: 不透露具體錯誤原因
          if (!isLoginRequest) {
            authService.clearAuth();
            notificationService.error('登入已過期，請重新登入');
            // 使用 setTimeout 確保在當前執行堆疊完成後再導航
            setTimeout(() => {
              router.navigate(['/login']);
            }, 0);
          }
          break;

        case 403:
          // 禁止存取 - 顯示權限不足訊息
          // 需求 3.3: 拒絕存取並顯示權限不足訊息
          notificationService.error('權限不足，無法執行此操作');
          break;

        case 404:
          // 找不到資源
          notificationService.error('找不到請求的資源');
          break;

        case 422:
          // 驗證錯誤 - 處理驗證錯誤訊息
          handleValidationErrors(error, notificationService);
          break;

        case 500:
          // 伺服器錯誤
          notificationService.error('伺服器發生錯誤，請稍後再試');
          break;

        case 0:
          // 網路錯誤或 CORS 問題
          notificationService.error('無法連線到伺服器，請檢查網路連線');
          break;

        default:
          // 其他錯誤
          if (error.status >= 400 && error.status < 500) {
            // 客戶端錯誤
            const message = getErrorMessage(error);
            notificationService.error(message);
          } else if (error.status >= 500) {
            // 伺服器錯誤
            notificationService.error('發生未預期的錯誤，請稍後再試');
          }
          break;
      }

      return throwError(() => error);
    })
  );
};

/**
 * 處理 422 驗證錯誤
 * @param error HTTP 錯誤回應
 * @param notificationService 通知服務
 */
function handleValidationErrors(
  error: HttpErrorResponse,
  notificationService: NotificationService
): void {
  const errorBody = error.error;

  if (errorBody) {
    // 處理標準錯誤格式 { message: string, errors: { [field]: string[] } }
    if (errorBody.errors && typeof errorBody.errors === 'object') {
      const errorMessages: string[] = [];
      
      for (const field in errorBody.errors) {
        if (Array.isArray(errorBody.errors[field])) {
          errorMessages.push(...errorBody.errors[field]);
        }
      }

      if (errorMessages.length > 0) {
        // 顯示第一個錯誤訊息
        notificationService.error(errorMessages[0]);
        return;
      }
    }

    // 處理單一錯誤訊息格式 { message: string }
    if (errorBody.message) {
      notificationService.error(errorBody.message);
      return;
    }

    // 處理字串格式
    if (typeof errorBody === 'string') {
      notificationService.error(errorBody);
      return;
    }
  }

  // 預設錯誤訊息
  notificationService.error('輸入資料驗證失敗，請檢查後重試');
}

/**
 * 從錯誤回應中取得錯誤訊息
 * @param error HTTP 錯誤回應
 * @returns 錯誤訊息
 */
function getErrorMessage(error: HttpErrorResponse): string {
  const errorBody = error.error;

  if (errorBody) {
    if (errorBody.message) {
      return errorBody.message;
    }
    if (typeof errorBody === 'string') {
      return errorBody;
    }
  }

  return '發生未預期的錯誤，請稍後再試';
}
