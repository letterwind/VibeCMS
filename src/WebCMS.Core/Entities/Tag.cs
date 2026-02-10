namespace WebCMS.Core.Entities;

/// <summary>
/// 標籤實體
/// </summary>
public class Tag
{
    /// <summary>
    /// 標籤 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 標籤名稱（唯一）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 文章標籤關聯（導覽屬性）
    /// </summary>
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
