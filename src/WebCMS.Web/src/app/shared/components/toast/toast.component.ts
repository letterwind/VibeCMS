import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Toast } from './notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.scss'
})
export class ToastComponent {
  notificationService = inject(NotificationService);

  getToastClass(toast: Toast): string {
    return `border-${toast.type === 'error' ? 'danger' : toast.type}`;
  }

  getHeaderClass(toast: Toast): string {
    const classes: Record<string, string> = {
      success: 'bg-success text-white',
      error: 'bg-danger text-white',
      warning: 'bg-warning text-dark',
      info: 'bg-info text-white'
    };
    return classes[toast.type] || 'bg-secondary text-white';
  }

  getIconClass(toast: Toast): string {
    const icons: Record<string, string> = {
      success: 'bi bi-check-circle-fill',
      error: 'bi bi-exclamation-triangle-fill',
      warning: 'bi bi-exclamation-circle-fill',
      info: 'bi bi-info-circle-fill'
    };
    return icons[toast.type] || 'bi bi-bell-fill';
  }

  getTitle(toast: Toast): string {
    const titles: Record<string, string> = {
      success: '成功',
      error: '錯誤',
      warning: '警告',
      info: '提示'
    };
    return titles[toast.type] || '通知';
  }

  dismiss(toast: Toast): void {
    this.notificationService.dismiss(toast.id);
  }
}
