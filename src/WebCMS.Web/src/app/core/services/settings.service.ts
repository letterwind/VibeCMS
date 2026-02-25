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
    return this.api.get<SiteSettingsDto>('/sitesettings');
  }

  updateSiteSettings(request: UpdateSiteSettingsRequest): Observable<SiteSettingsDto> {
    return this.api.put<SiteSettingsDto>('/sitesettings', request);
  }

  uploadFavicon(file: File): Observable<{ faviconPath: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.api.postFormData<{ faviconPath: string }>('/sitesettings/favicon', formData);
  }

  // Header Settings
  getHeaderSettings(): Observable<HtmlSettingsDto> {
    return this.api.get<HtmlSettingsDto>('/headersettings');
  }

  updateHeaderSettings(request: UpdateHtmlSettingsRequest): Observable<HtmlSettingsDto> {
    return this.api.put<HtmlSettingsDto>('/headersettings', request);
  }

  // Footer Settings
  getFooterSettings(): Observable<HtmlSettingsDto> {
    return this.api.get<HtmlSettingsDto>('/footersettings');
  }

  updateFooterSettings(request: UpdateHtmlSettingsRequest): Observable<HtmlSettingsDto> {
    return this.api.put<HtmlSettingsDto>('/footersettings', request);
  }
}
