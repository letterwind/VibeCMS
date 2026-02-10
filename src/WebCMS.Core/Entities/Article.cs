namespace WebCMS.Core.Entities;

/// <summary>
/// 文章實體
/// </summary>
public class Article : BaseEntity
{
    /// <summary>
    /// 文章標題（最多 200 字元）
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 文章內容（HTML 格式，無長度限制）
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 自訂網址代稱（唯一）
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// 分類 ID
    /// </summary>
    public int CategoryId { get; set; }

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
    /// 建立者 ID
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// 更新者 ID
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// 分類（導覽屬性）
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// 建立者（導覽屬性）
    /// </summary>
    public AdminUser? Creator { get; set; }

    /// <summary>
    /// 更新者（導覽屬性）
    /// </summary>
    public AdminUser? Updater { get; set; }

    /// <summary>
    /// 文章標籤關聯（導覽屬性）
    /// </summary>
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
