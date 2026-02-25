import { Component, Output, EventEmitter, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { LanguageSelectorComponent } from '../../shared/components/language-selector/language-selector.component';

type DeviceType = 'mobile' | 'tablet' | 'desktop';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, LanguageSelectorComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  @Output() toggleSidebar = new EventEmitter<void>();
  @Input() deviceType: DeviceType = 'desktop';
  
  private authService = inject(AuthService);
  private router = inject(Router);
  
  readonly currentUser = this.authService.currentUser;
  
  onLogout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: () => {
        this.authService.clearAuth();
        this.router.navigate(['/login']);
      }
    });
  }
}
