namespace WebCMS.Core.Entities;

/// <summary>
/// 頁首設定實體（單一記錄）
/// </summary>
public class HeaderSettings
{
    public int Id { get; set; }

    /// <summary>
    /// HTML 內容
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
