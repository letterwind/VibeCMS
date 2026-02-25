/**
 * 語言資源 DTO
 */
export interface LanguageResource
{
    id: number;
    languageCode: string;
    resourceKey: string;
    resourceValue: string;
    resourceType: string;
    description?: string;
    createdAt: Date;
    updatedAt: Date;
    createdBy?: string;
    updatedBy?: string;
}

/**
 * 建立/更新語言資源請求
 */
export interface CreateOrUpdateLanguageResourceRequest
{
    languageCode: string;
    resourceKey: string;
    resourceValue: string;
    resourceType: string;
    description?: string;
}

/**
 * 語言檔匯出/匯入 DTO
 */
export interface LanguageFileExport
{
    languageCode: string;
    languageName: string;
    resources: Record<string, any>;
    exportedAt: Date;
}

/**
 * 語言檔匯入請求
 */
export interface LanguageFileImportRequest
{
    languageCode: string;
    fileContent: string;
    overwrite?: boolean;
}

/**
 * API 響應格式
 */
export interface LanguageResourceResponse<T = any>
{
    success: boolean;
    data?: T;
    message?: string;
}
