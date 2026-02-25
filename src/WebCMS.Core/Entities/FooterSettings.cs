namespace WebCMS.Core.Entities;

/// <summary>
/// 頁尾設定實體（每種語言一筆記錄）
/// </summary>
public class FooterSettings
{
    public int Id { get; set; }

    /// <summary>
    /// 語言代碼（如 zh-TW、en-US、ja-JP）
    /// </summary>
    public string LanguageCode { get; set; } = "zh-TW";

    /// <summary>
    /// HTML 內容
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
