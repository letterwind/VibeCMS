using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Exceptions;

/// <summary>
/// 禁止存取例外
/// 當使用者已登入但沒有權限存取特定資源時拋出
/// 對應 HTTP 403 Forbidden
/// 驗證: 需求 3.3
/// </summary>
public class ForbiddenException : BaseException
{
    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="message">錯誤訊息</param>
    public ForbiddenException(string message = "權限不足，無法執行此操作") 
        : base(ErrorCodes.AccessDenied, message)
    {
    }

    /// <summary>
    /// 建構子（含自訂錯誤代碼）
    /// </summary>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    public ForbiddenException(string errorCode, string message) 
        : base(errorCode, message)
    {
    }

    /// <summary>
    /// 建立存取被拒絕的例外
    /// </summary>
    public static ForbiddenException AccessDenied()
    {
        return new ForbiddenException(ErrorCodes.AccessDenied, "存取被拒絕");
    }

    /// <summary>
    /// 建立權限不足的例外
    /// </summary>
    /// <param name="functionName">功能名稱</param>
    /// <param name="operation">操作類型</param>
    public static ForbiddenException InsufficientPermission(string? functionName = null, string? operation = null)
    {
        var message = "權限不足";
        if (!string.IsNullOrEmpty(functionName) && !string.IsNullOrEmpty(operation))
        {
            message = $"您沒有「{functionName}」的「{operation}」權限";
        }
        else if (!string.IsNullOrEmpty(functionName))
        {
            message = $"您沒有「{functionName}」的存取權限";
        }
        
        return new ForbiddenException(ErrorCodes.InsufficientPermission, message);
    }
}
