namespace WebCMS.Core.DTOs.Settings;

/// <summary>
/// 更新 HTML 設定請求（用於頁首/頁尾設定）
/// </summary>
public record UpdateHtmlSettingsRequest(
    string? HtmlContent
);
