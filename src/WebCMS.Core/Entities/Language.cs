namespace WebCMS.Core.Entities;

/// <summary>
/// 語言設定實體
/// </summary>
public class Language
{
    public int Id { get; set; }
    
    /// <summary>
    /// 語言代碼（如 zh-TW、en-US、ja-JP）
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 語言名稱（如 繁體中文、English、日本語）
    /// </summary>
    public string LanguageName { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
