namespace WebCMS.Core.Exceptions;

/// <summary>
/// 基礎例外類別
/// 所有自訂例外的基底類別
/// </summary>
public abstract class BaseException : Exception
{
    /// <summary>
    /// 錯誤代碼
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    protected BaseException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 建構子（含內部例外）
    /// </summary>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    /// <param name="innerException">內部例外</param>
    protected BaseException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
