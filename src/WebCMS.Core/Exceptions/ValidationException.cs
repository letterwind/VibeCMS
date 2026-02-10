using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Exceptions;

/// <summary>
/// 驗證例外
/// 當輸入資料驗證失敗時拋出
/// </summary>
public class ValidationException : BaseException
{
    /// <summary>
    /// 驗證錯誤詳細資訊
    /// Key: 欄位名稱, Value: 錯誤訊息陣列
    /// </summary>
    public Dictionary<string, string[]> Errors { get; }

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="message">錯誤訊息</param>
    public ValidationException(string message) 
        : base(ErrorCodes.ValidationFailed, message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// 建構子（含錯誤詳細資訊）
    /// </summary>
    /// <param name="message">錯誤訊息</param>
    /// <param name="errors">驗證錯誤詳細資訊</param>
    public ValidationException(string message, Dictionary<string, string[]> errors) 
        : base(ErrorCodes.ValidationFailed, message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// 建構子（含單一欄位錯誤）
    /// </summary>
    /// <param name="fieldName">欄位名稱</param>
    /// <param name="errorMessage">錯誤訊息</param>
    public ValidationException(string fieldName, string errorMessage) 
        : base(ErrorCodes.ValidationFailed, errorMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            { fieldName, new[] { errorMessage } }
        };
    }

    /// <summary>
    /// 建構子（含自訂錯誤代碼）
    /// </summary>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    /// <param name="errors">驗證錯誤詳細資訊</param>
    public ValidationException(string errorCode, string message, Dictionary<string, string[]>? errors) 
        : base(errorCode, message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}
