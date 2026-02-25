namespace WebCMS.Core.DTOs.Article;

/// <summary>
/// 文章 DTO
/// </summary>
public record ArticleDto(
    int Id,
    string Title,
    string Content,
    string Slug,
    string LanguageCode,
    int CategoryId,
    string? CategoryName,
    List<string> Tags,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? CreatedBy,
    string? CreatedByName,
    /// <summary>
    /// 翻譯狀態（鍵為語言代碼，值為是否已翻譯）
    /// </summary>
    Dictionary<string, bool>? AvailableLanguages = null
);
