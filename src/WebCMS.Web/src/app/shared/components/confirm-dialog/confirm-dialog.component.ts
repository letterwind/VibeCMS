import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ConfirmDialogType = 'danger' | 'warning' | 'info';

export interface ConfirmDialogOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: ConfirmDialogType;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss'
})
export class ConfirmDialogComponent {
  @Input() isOpen = false;
  @Input() title = '確認';
  @Input() message = '';
  @Input() confirmText = '確定';
  @Input() cancelText = '取消';
  @Input() type: ConfirmDialogType = 'warning';

  @Output() onConfirm = new EventEmitter<void>();
  @Output() onCancel = new EventEmitter<void>();

  get headerClass(): string {
    const classes: Record<ConfirmDialogType, string> = {
      danger: 'bg-danger text-white',
      warning: 'bg-warning',
      info: 'bg-info text-white'
    };
    return classes[this.type];
  }

  get iconClass(): string {
    const icons: Record<ConfirmDialogType, string> = {
      danger: 'bi bi-exclamation-triangle-fill',
      warning: 'bi bi-exclamation-circle-fill',
      info: 'bi bi-info-circle-fill'
    };
    return icons[this.type];
  }

  get confirmButtonClass(): string {
    const classes: Record<ConfirmDialogType, string> = {
      danger: 'btn btn-danger',
      warning: 'btn btn-warning',
      info: 'btn btn-info'
    };
    return classes[this.type];
  }

  confirm(): void {
    this.onConfirm.emit();
    this.isOpen = false;
  }

  cancel(): void {
    this.onCancel.emit();
    this.isOpen = false;
  }
}
