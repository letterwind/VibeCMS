using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.Settings;

/// <summary>
/// 更新網站設定請求
/// </summary>
public record UpdateSiteSettingsRequest(
    [MaxLength(100)] string? MetaTitle,
    [MaxLength(200)] string? MetaDescription,
    [MaxLength(200)] string? MetaKeywords
);
