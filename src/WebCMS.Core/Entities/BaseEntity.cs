namespace WebCMS.Core.Entities;

/// <summary>
/// 基礎實體類別，提供所有實體共用的屬性
/// </summary>
public abstract class BaseEntity : ISoftDeletable
{
    public int Id { get; set; }
    
    /// <summary>
    /// 語言代碼（如 zh-TW、en-US、ja-JP）
    /// </summary>
    public string LanguageCode { get; set; } = "zh-TW";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
