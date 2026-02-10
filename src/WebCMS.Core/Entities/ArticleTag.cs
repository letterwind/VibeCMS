namespace WebCMS.Core.Entities;

/// <summary>
/// 文章標籤關聯實體（多對多）
/// </summary>
public class ArticleTag
{
    /// <summary>
    /// 文章 ID
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// 標籤 ID
    /// </summary>
    public int TagId { get; set; }

    /// <summary>
    /// 文章（導覽屬性）
    /// </summary>
    public Article? Article { get; set; }

    /// <summary>
    /// 標籤（導覽屬性）
    /// </summary>
    public Tag? Tag { get; set; }
}
