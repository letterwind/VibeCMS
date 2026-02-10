using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Exceptions;

/// <summary>
/// 業務邏輯例外
/// 當業務規則驗證失敗時拋出
/// 對應 HTTP 422 Unprocessable Entity
/// </summary>
public class BusinessException : BaseException
{
    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    public BusinessException(string errorCode, string message) 
        : base(errorCode, message)
    {
    }

    /// <summary>
    /// 建構子（含內部例外）
    /// </summary>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    /// <param name="innerException">內部例外</param>
    public BusinessException(string errorCode, string message, Exception innerException) 
        : base(errorCode, message, innerException)
    {
    }

    /// <summary>
    /// 建立分類深度超過限制的例外
    /// </summary>
    public static BusinessException MaxCategoryDepthExceeded()
    {
        return new BusinessException(
            ErrorCodes.MaxCategoryDepthExceeded, 
            "分類層級已達上限（最多 3 層）");
    }

    /// <summary>
    /// 建立無法刪除含有子項目的例外
    /// </summary>
    /// <param name="resourceType">資源類型</param>
    public static BusinessException CannotDeleteWithChildren(string resourceType)
    {
        return new BusinessException(
            ErrorCodes.CannotDeleteWithChildren, 
            $"無法刪除此{resourceType}，因為它包含子項目");
    }

    /// <summary>
    /// 建立單一記錄限制的例外
    /// </summary>
    /// <param name="settingType">設定類型</param>
    public static BusinessException SingleRecordOnly(string settingType)
    {
        return new BusinessException(
            ErrorCodes.SingleRecordOnly, 
            $"{settingType}僅允許單一記錄，不可新增或刪除");
    }

    /// <summary>
    /// 建立新密碼與目前密碼相同的例外
    /// </summary>
    public static BusinessException PasswordSameAsCurrent()
    {
        return new BusinessException(
            ErrorCodes.PasswordSameAsCurrent, 
            "新密碼不得與目前密碼相同");
    }

    /// <summary>
    /// 建立密碼包含帳號的例外
    /// </summary>
    public static BusinessException PasswordContainsAccount()
    {
        return new BusinessException(
            ErrorCodes.PasswordContainsAccount, 
            "密碼不得包含帳號內容");
    }

    /// <summary>
    /// 建立資源已存在的例外
    /// </summary>
    /// <param name="resourceType">資源類型</param>
    /// <param name="identifier">識別資訊</param>
    public static BusinessException ResourceAlreadyExists(string resourceType, string identifier)
    {
        return new BusinessException(
            ErrorCodes.ResourceAlreadyExists, 
            $"{resourceType}「{identifier}」已存在");
    }

    /// <summary>
    /// 建立資源使用中的例外
    /// </summary>
    /// <param name="resourceType">資源類型</param>
    public static BusinessException ResourceInUse(string resourceType)
    {
        return new BusinessException(
            ErrorCodes.ResourceInUse, 
            $"此{resourceType}正在使用中，無法刪除");
    }
}
