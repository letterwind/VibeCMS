/**
 * 語言模型
 */
export interface Language
{
    id: number;
    languageCode: string;
    languageName: string;
    isActive: boolean;
    sortOrder: number;
}

/**
 * 翻譯狀態
 */
export interface TranslationStatus
{
    entityId: number;
    entityType: string;
    languageStatus: Record<string, boolean>;
    completionPercentage: number;
}
