namespace WebCMS.Core.Entities;

/// <summary>
/// 網站設定實體（每種語言一筆記錄）
/// </summary>
public class SiteSettings
{
    public int Id { get; set; }

    /// <summary>
    /// 語言代碼（如 zh-TW、en-US、ja-JP）
    /// </summary>
    public string LanguageCode { get; set; } = "zh-TW";

    /// <summary>
    /// SEO 標題
    /// </summary>
    public string? MetaTitle { get; set; }

    /// <summary>
    /// SEO 描述
    /// </summary>
    public string? MetaDescription { get; set; }

    /// <summary>
    /// SEO 關鍵字
    /// </summary>
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// Favicon 檔案路徑
    /// </summary>
    public string? FaviconPath { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
