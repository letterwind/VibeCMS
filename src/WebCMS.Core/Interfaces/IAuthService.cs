using WebCMS.Core.DTOs.Auth;
using WebCMS.Core.Entities;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 認證服務介面
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 驗證登入憑證
    /// </summary>
    Task<LoginResult> ValidateCredentialsAsync(string account, string password, string captcha, string captchaToken);

    /// <summary>
    /// 檢查帳號是否被鎖定
    /// </summary>
    Task<bool> IsAccountLockedAsync(string account);

    /// <summary>
    /// 鎖定帳號
    /// </summary>
    Task LockAccountAsync(string account);

    /// <summary>
    /// 解鎖帳號
    /// </summary>
    Task UnlockAccountAsync(string account);

    /// <summary>
    /// 取得登入失敗次數
    /// </summary>
    Task<int> GetFailedAttemptsAsync(string account);

    /// <summary>
    /// 增加登入失敗次數
    /// </summary>
    Task IncrementFailedAttemptsAsync(string account);

    /// <summary>
    /// 重設登入失敗次數
    /// </summary>
    Task ResetFailedAttemptsAsync(string account);

    /// <summary>
    /// 產生 JWT Token
    /// </summary>
    string GenerateJwtToken(AdminUser user);

    /// <summary>
    /// 產生 Refresh Token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// 檢查密碼是否過期
    /// </summary>
    bool IsPasswordExpired(DateTime passwordLastChanged);

    /// <summary>
    /// 變更密碼
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

    /// <summary>
    /// 記錄登入嘗試
    /// </summary>
    Task RecordLoginAttemptAsync(string account, bool isSuccess, string? ipAddress);
}

/// <summary>
/// 登入結果
/// </summary>
public record LoginResult(
    bool Success,
    string? ErrorMessage,
    AdminUser? User = null,
    bool IsPasswordExpired = false
);
