namespace WebCMS.Core.DTOs.Category;

/// <summary>
/// 分類 DTO
/// </summary>
public record CategoryDto(
    int Id,
    string Name,
    string Slug,
    string LanguageCode,
    int? ParentId,
    int Level,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    int SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    /// <summary>
    /// 翻譯狀態（鍵為語言代碼，值為是否已翻譯）
    /// </summary>
    Dictionary<string, bool>? AvailableLanguages = null
);
