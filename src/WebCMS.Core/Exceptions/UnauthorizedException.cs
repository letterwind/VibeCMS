using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Exceptions;

/// <summary>
/// 未授權例外
/// 當使用者未登入或認證失敗時拋出
/// 對應 HTTP 401 Unauthorized
/// </summary>
public class UnauthorizedException : BaseException
{
    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="message">錯誤訊息</param>
    public UnauthorizedException(string message = "未授權的存取") 
        : base(ErrorCodes.InvalidCredentials, message)
    {
    }

    /// <summary>
    /// 建構子（含自訂錯誤代碼）
    /// </summary>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    public UnauthorizedException(string errorCode, string message) 
        : base(errorCode, message)
    {
    }

    /// <summary>
    /// 建立帳號或密碼錯誤的例外
    /// 注意：為了安全性，不透露具體是帳號還是密碼錯誤
    /// 驗證: 需求 1.6
    /// </summary>
    public static UnauthorizedException InvalidCredentials()
    {
        return new UnauthorizedException(ErrorCodes.InvalidCredentials, "帳號或密碼錯誤");
    }

    /// <summary>
    /// 建立帳號被鎖定的例外
    /// </summary>
    public static UnauthorizedException AccountLocked()
    {
        return new UnauthorizedException(ErrorCodes.AccountLocked, "帳號已被鎖定，請稍後再試");
    }

    /// <summary>
    /// 建立驗證碼錯誤的例外
    /// </summary>
    public static UnauthorizedException InvalidCaptcha()
    {
        return new UnauthorizedException(ErrorCodes.InvalidCaptcha, "驗證碼錯誤");
    }

    /// <summary>
    /// 建立 Token 過期的例外
    /// </summary>
    public static UnauthorizedException TokenExpired()
    {
        return new UnauthorizedException(ErrorCodes.TokenExpired, "登入已過期，請重新登入");
    }

    /// <summary>
    /// 建立密碼過期的例外
    /// </summary>
    public static UnauthorizedException PasswordExpired()
    {
        return new UnauthorizedException(ErrorCodes.PasswordExpired, "密碼已過期，請變更密碼");
    }
}
