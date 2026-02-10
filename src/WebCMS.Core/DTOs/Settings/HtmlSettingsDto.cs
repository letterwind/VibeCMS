namespace WebCMS.Core.DTOs.Settings;

/// <summary>
/// HTML 設定 DTO（用於頁首/頁尾設定）
/// </summary>
public record HtmlSettingsDto(
    int Id,
    string? HtmlContent,
    DateTime UpdatedAt
);
