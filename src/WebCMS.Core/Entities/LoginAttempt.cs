namespace WebCMS.Core.Entities;

/// <summary>
/// 登入嘗試記錄實體
/// </summary>
public class LoginAttempt
{
    public int Id { get; set; }

    /// <summary>
    /// 嘗試登入的帳號
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 是否登入成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 登入來源 IP 位址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 嘗試時間
    /// </summary>
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}
