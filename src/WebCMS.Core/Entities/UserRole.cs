namespace WebCMS.Core.Entities;

/// <summary>
/// 使用者角色關聯實體（多對多關聯）
/// </summary>
public class UserRole
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 使用者
    /// </summary>
    public AdminUser User { get; set; } = null!;

    /// <summary>
    /// 角色 ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    public Role Role { get; set; } = null!;
}
