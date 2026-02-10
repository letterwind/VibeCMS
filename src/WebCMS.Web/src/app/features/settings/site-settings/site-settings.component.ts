import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../../core/services/settings.service';
import { SiteSettingsDto, UpdateSiteSettingsRequest } from '../../../core/models/settings.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-site-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './site-settings.component.html',
  styleUrl: './site-settings.component.scss'
})
export class SiteSettingsComponent implements OnInit {
  settings: SiteSettingsDto = {
    id: 0,
    metaTitle: null,
    metaDescription: null,
    metaKeywords: null,
    faviconPath: null,
    faviconUrl: null,
    updatedAt: new Date()
  };
  
  loading = true;
  saving = false;
  message = '';
  isError = false;
  selectedFile: File | null = null;

  constructor(private settingsService: SettingsService) {}

  ngOnInit(): void {
    this.loadSettings();
  }

  getFaviconUrl(): string {
    if (!this.settings.faviconPath) {
      return '';
    }
    if (this.settings.faviconPath.startsWith('http')) {
      return this.settings.faviconPath;
    }
    return `${environment.apiUrl}${this.settings.faviconPath}`;
  }

  loadSettings(): void {
    this.loading = true;
    this.settingsService.getSiteSettings().subscribe({
      next: (result) => {
        this.settings = result;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load site settings:', err);
        this.showMessage('載入設定失敗', true);
        this.loading = false;
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  save(): void {
    this.saving = true;
    this.message = '';

    if (this.selectedFile) {
      this.settingsService.uploadFavicon(this.selectedFile).subscribe({
        next: (result) => {
          this.settings.faviconPath = result.faviconPath;
          this.settings.faviconUrl = result.faviconPath;
          this.selectedFile = null;
          this.saveSettings();
        },
        error: (err) => {
          console.error('Failed to upload favicon:', err);
          this.showMessage('上傳 Favicon 失敗', true);
          this.saving = false;
        }
      });
    } else {
      this.saveSettings();
    }
  }

  private saveSettings(): void {
    const request: UpdateSiteSettingsRequest = {
      metaTitle: this.settings.metaTitle,
      metaDescription: this.settings.metaDescription,
      metaKeywords: this.settings.metaKeywords
    };

    this.settingsService.updateSiteSettings(request).subscribe({
      next: (result) => {
        this.settings = result;
        this.showMessage('設定已儲存', false);
        this.saving = false;
      },
      error: (err) => {
        console.error('Failed to save site settings:', err);
        this.showMessage('儲存設定失敗', true);
        this.saving = false;
      }
    });
  }

  private showMessage(msg: string, isError: boolean): void {
    this.message = msg;
    this.isError = isError;
    setTimeout(() => {
      this.message = '';
    }, 5000);
  }
}
