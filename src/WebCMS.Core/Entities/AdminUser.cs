namespace WebCMS.Core.Entities;

/// <summary>
/// 管理員使用者實體
/// </summary>
public class AdminUser : BaseEntity
{
    /// <summary>
    /// 帳號（唯一）
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 密碼雜湊值
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 顯示名稱
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 密碼最後變更時間
    /// </summary>
    public DateTime PasswordLastChanged { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 登入失敗次數
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// 鎖定結束時間
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// 使用者角色關聯
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
