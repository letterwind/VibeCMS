namespace WebCMS.Core.Entities;

/// <summary>
/// 文章分類實體
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// 分類名稱（最多 20 字元）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 自訂網址代稱（唯一）
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// 父分類 ID（用於建立樹狀結構）
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 分類層級（1-3，最多 3 層）
    /// </summary>
    public int Level { get; set; } = 1;

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
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 父分類（導覽屬性）
    /// </summary>
    public Category? Parent { get; set; }

    /// <summary>
    /// 子分類集合（導覽屬性）
    /// </summary>
    public ICollection<Category> Children { get; set; } = new List<Category>();
}
