import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../../core/services/settings.service';
import { HtmlSettingsDto, UpdateHtmlSettingsRequest } from '../../../core/models/settings.model';
import { EditorComponent } from '@tinymce/tinymce-angular';

@Component({
  selector: 'app-footer-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, EditorComponent],
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

  editorInit: EditorComponent['init'] = {
    base_url: '/assets/js/tinymce',
    suffix: '.min',
    height: 400,
    menubar: true,
    plugins: [
      'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
      'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
      'insertdatetime', 'media', 'table', 'help', 'wordcount'
    ],
    toolbar: 'undo redo | blocks | bold italic forecolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | removeformat | link image | code | help',
    content_style: 'body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; font-size: 14px; }'
  };

  constructor(private settingsService: SettingsService) {}

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
        this.showMessage('載入設定失敗', true);
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
        this.showMessage('設定已儲存', false);
        this.saving = false;
      },
      error: (err) => {
        console.error('Failed to save footer settings:', err);
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
