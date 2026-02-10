import { Component, HostListener, OnInit, OnDestroy, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { FooterComponent } from './footer/footer.component';
import { filter, Subscription } from 'rxjs';

const BREAKPOINTS = {
  SM: 576,
  MD: 768,
  LG: 992,
  XL: 1200,
  XXL: 1400
};

type DeviceType = 'mobile' | 'tablet' | 'desktop';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    HeaderComponent,
    SidebarComponent,
    FooterComponent
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent implements OnInit, OnDestroy {
  private platformId = inject(PLATFORM_ID);
  private router = inject(Router);
  private routerSubscription?: Subscription;
  
  sidebarOpen = false;
  deviceType: DeviceType = 'desktop';
  
  get isSidebarVisible(): boolean {
    return this.deviceType === 'desktop' || this.sidebarOpen;
  }
  
  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.updateDeviceType();
      
      this.routerSubscription = this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        if (this.deviceType !== 'desktop') {
          this.closeSidebar();
        }
      });
    }
  }
  
  ngOnDestroy(): void {
    this.routerSubscription?.unsubscribe();
  }
  
  @HostListener('window:resize')
  onResize(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.updateDeviceType();
    }
  }
  
  @HostListener('window:keydown.escape')
  onEscapeKey(): void {
    if (this.sidebarOpen && this.deviceType !== 'desktop') {
      this.closeSidebar();
    }
  }
  
  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
    this.updateBodyClass();
  }
  
  closeSidebar(): void {
    this.sidebarOpen = false;
    this.updateBodyClass();
  }
  
  private updateDeviceType(): void {
    const width = window.innerWidth;
    
    if (width < BREAKPOINTS.MD) {
      this.deviceType = 'mobile';
    } else if (width < BREAKPOINTS.LG) {
      this.deviceType = 'tablet';
    } else {
      this.deviceType = 'desktop';
    }
    
    if (this.deviceType === 'desktop') {
      this.sidebarOpen = false;
      this.updateBodyClass();
    }
  }
  
  private updateBodyClass(): void {
    if (isPlatformBrowser(this.platformId)) {
      if (this.sidebarOpen && this.deviceType !== 'desktop') {
        document.body.classList.add('sidebar-open');
      } else {
        document.body.classList.remove('sidebar-open');
      }
    }
  }
}
