namespace WebCMS.Core.DTOs.Settings;

/// <summary>
/// 網站設定 DTO
/// </summary>
public record SiteSettingsDto(
    int Id,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? FaviconPath,
    string? FaviconUrl,
    DateTime UpdatedAt
);
