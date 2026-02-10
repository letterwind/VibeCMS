export interface SiteSettingsDto {
  id: number;
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
  faviconPath: string | null;
  faviconUrl: string | null;
  updatedAt: Date;
}

export interface HtmlSettingsDto {
  id: number;
  htmlContent: string | null;
  updatedAt: Date;
}

export interface UpdateSiteSettingsRequest {
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
}

export interface UpdateHtmlSettingsRequest {
  htmlContent: string | null;
}
