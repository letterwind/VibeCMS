namespace WebCMS.Core.Entities;

/// <summary>
/// 功能選單實體
/// </summary>
public class Function : BaseEntity
{
    /// <summary>
    /// 功能名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 功能代碼（唯一）
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 連結網址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 是否在新視窗開啟
    /// </summary>
    public bool OpenInNewWindow { get; set; } = false;

    /// <summary>
    /// Bootstrap Icons 圖示名稱
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 父功能 ID（用於建立樹狀結構）
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 父功能（導覽屬性）
    /// </summary>
    public Function? Parent { get; set; }

    /// <summary>
    /// 子功能集合（導覽屬性）
    /// </summary>
    public ICollection<Function> Children { get; set; } = new List<Function>();

    /// <summary>
    /// 角色權限關聯
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
