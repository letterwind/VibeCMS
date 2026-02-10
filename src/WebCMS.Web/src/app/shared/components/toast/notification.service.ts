import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
  duration: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private toastsSignal = signal<Toast[]>([]);
  private nextId = 1;

  readonly toasts = this.toastsSignal.asReadonly();

  /**
   * 顯示成功訊息
   * @param message 訊息內容
   * @param duration 顯示時間（毫秒），預設 3000
   */
  success(message: string, duration = 3000): void {
    this.show(message, 'success', duration);
  }

  /**
   * 顯示錯誤訊息
   * @param message 訊息內容
   * @param duration 顯示時間（毫秒），預設 5000
   */
  error(message: string, duration = 5000): void {
    this.show(message, 'error', duration);
  }

  /**
   * 顯示警告訊息
   * @param message 訊息內容
   * @param duration 顯示時間（毫秒），預設 4000
   */
  warning(message: string, duration = 4000): void {
    this.show(message, 'warning', duration);
  }

  /**
   * 顯示提示訊息
   * @param message 訊息內容
   * @param duration 顯示時間（毫秒），預設 3000
   */
  info(message: string, duration = 3000): void {
    this.show(message, 'info', duration);
  }

  /**
   * 顯示通知訊息
   * @param message 訊息內容
   * @param type 訊息類型
   * @param duration 顯示時間（毫秒）
   */
  show(message: string, type: ToastType, duration: number): void {
    const id = this.nextId++;
    const toast: Toast = { id, message, type, duration };
    
    this.toastsSignal.update(toasts => [...toasts, toast]);

    // 自動移除
    if (duration > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, duration);
    }
  }

  /**
   * 關閉指定的通知
   * @param id 通知 ID
   */
  dismiss(id: number): void {
    this.toastsSignal.update(toasts => toasts.filter(t => t.id !== id));
  }

  /**
   * 關閉所有通知
   */
  dismissAll(): void {
    this.toastsSignal.set([]);
  }
}
