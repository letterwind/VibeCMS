namespace WebCMS.Core.Entities;

/// <summary>
/// 角色實體
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// 角色名稱（唯一）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 階層等級（數字越小權限越高，例如：1=Admin, 2=Manager, 3=Finance, 4=User）
    /// </summary>
    public int HierarchyLevel { get; set; }

    /// <summary>
    /// 使用者角色關聯
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// 角色權限關聯
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
