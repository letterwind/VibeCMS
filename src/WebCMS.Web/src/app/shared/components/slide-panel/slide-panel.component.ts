import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SlidePanelSize = 'sm' | 'md' | 'lg' | 'xl';

@Component({
  selector: 'app-slide-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './slide-panel.component.html',
  styleUrl: './slide-panel.component.scss'
})
export class SlidePanelComponent {
  @Input() isOpen = false;
  @Input() title = '';
  @Input() size: SlidePanelSize = 'md';
  @Input() closeOnBackdrop = true;
  
  @Output() onClose = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscapeKey() {
    if (this.isOpen) {
      this.close();
    }
  }

  open(): void {
    this.isOpen = true;
    document.body.style.overflow = 'hidden';
  }

  close(): void {
    this.isOpen = false;
    document.body.style.overflow = '';
    this.onClose.emit();
  }

  onBackdropClick(): void {
    if (this.closeOnBackdrop) {
      this.close();
    }
  }
}
