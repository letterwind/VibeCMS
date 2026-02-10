namespace WebCMS.Core.Entities;

/// <summary>
/// 角色權限實體 - 定義角色對功能的 CRUD 權限
/// </summary>
public class RolePermission
{
    /// <summary>
    /// 角色 ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// 功能 ID
    /// </summary>
    public int FunctionId { get; set; }

    /// <summary>
    /// 是否可新增
    /// </summary>
    public bool CanCreate { get; set; }

    /// <summary>
    /// 是否可讀取
    /// </summary>
    public bool CanRead { get; set; }

    /// <summary>
    /// 是否可更新
    /// </summary>
    public bool CanUpdate { get; set; }

    /// <summary>
    /// 是否可刪除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 角色（導覽屬性）
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// 功能（導覽屬性）
    /// </summary>
    public Function Function { get; set; } = null!;
}
