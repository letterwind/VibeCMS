import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../../core/services/settings.service';
import { HtmlSettingsDto, UpdateHtmlSettingsRequest } from '../../../core/models/settings.model';
import { EditorComponent } from '@tinymce/tinymce-angular';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { LanguageService } from '../../../core/services/language.service';
import { TinyMceConfigService } from '../../../core/services/tinymce-config.service';

@Component({
  selector: 'app-footer-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, EditorComponent, TranslatePipe],
  templateUrl: './footer-settings.component.html',
  styleUrl: './footer-settings.component.scss'
})
export class FooterSettingsComponent implements OnInit {
  settings: HtmlSettingsDto = {
    id: 0,
    htmlContent: null,
    updatedAt: new Date()
  };
  
  editorContent = '';
  loading = true;
  saving = false;
  message = '';
  isError = false;

  editorInit: EditorComponent['init'];

  constructor(
    private settingsService: SettingsService,
    private languageService: LanguageService,
    private tinyMceConfig: TinyMceConfigService
  ) {
    this.editorInit = this.tinyMceConfig.getConfig({ menubar: true });
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.loading = true;
    this.settingsService.getFooterSettings().subscribe({
      next: (result) => {
        this.settings = result;
        this.editorContent = result.htmlContent || '';
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load footer settings:', err);
        this.showMessage(this.languageService.getTranslation('footerSettings.loadFailed'), true);
        this.loading = false;
      }
    });
  }

  save(): void {
    this.saving = true;
    this.message = '';

    const request: UpdateHtmlSettingsRequest = {
      htmlContent: this.editorContent || null
    };

    this.settingsService.updateFooterSettings(request).subscribe({
      next: (result) => {
        this.settings = result;
        this.editorContent = result.htmlContent || '';
        this.showMessage(this.languageService.getTranslation('footerSettings.saveSuccess'), false);
        this.saving = false;
      },
      error: (err) => {
        console.error('Failed to save footer settings:', err);
        this.showMessage(this.languageService.getTranslation('footerSettings.saveFailed'), true);
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
