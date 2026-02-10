import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  SiteSettingsDto,
  HtmlSettingsDto,
  UpdateSiteSettingsRequest,
  UpdateHtmlSettingsRequest
} from '../models/settings.model';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  constructor(private api: ApiService) {}

  // Site Settings
  getSiteSettings(): Observable<SiteSettingsDto> {
    return this.api.get<SiteSettingsDto>('/api/sitesettings');
  }

  updateSiteSettings(request: UpdateSiteSettingsRequest): Observable<SiteSettingsDto> {
    return this.api.put<SiteSettingsDto>('/api/sitesettings', request);
  }

  uploadFavicon(file: File): Observable<{ faviconPath: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.api.postFormData<{ faviconPath: string }>('/api/sitesettings/favicon', formData);
  }

  // Header Settings
  getHeaderSettings(): Observable<HtmlSettingsDto> {
    return this.api.get<HtmlSettingsDto>('/api/headersettings');
  }

  updateHeaderSettings(request: UpdateHtmlSettingsRequest): Observable<HtmlSettingsDto> {
    return this.api.put<HtmlSettingsDto>('/api/headersettings', request);
  }

  // Footer Settings
  getFooterSettings(): Observable<HtmlSettingsDto> {
    return this.api.get<HtmlSettingsDto>('/api/footersettings');
  }

  updateFooterSettings(request: UpdateHtmlSettingsRequest): Observable<HtmlSettingsDto> {
    return this.api.put<HtmlSettingsDto>('/api/footersettings', request);
  }
}
