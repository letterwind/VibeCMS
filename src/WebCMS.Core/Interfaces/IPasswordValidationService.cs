namespace WebCMS.Core.Interfaces;

/// <summary>
/// 密碼驗證服務介面
/// </summary>
public interface IPasswordValidationService
{
    /// <summary>
    /// 驗證帳號格式
    /// 規則：6-12 字元，必須包含至少 1 個大寫字母、1 個小寫字母、1 個數字
    /// </summary>
    /// <param name="account">帳號</param>
    /// <returns>驗證結果</returns>
    ValidationResult ValidateAccount(string account);

    /// <summary>
    /// 驗證密碼格式
    /// 規則：6-12 字元，必須包含至少 1 個大寫字母、1 個小寫字母、1 個數字
    /// 額外規則：密碼不得包含帳號內容
    /// </summary>
    /// <param name="password">密碼</param>
    /// <param name="account">帳號（用於檢查密碼是否包含帳號）</param>
    /// <returns>驗證結果</returns>
    ValidationResult ValidatePassword(string password, string account);

    /// <summary>
    /// 檢查密碼是否過期
    /// 規則：密碼使用超過 3 個月視為過期
    /// </summary>
    /// <param name="lastPasswordChange">上次密碼變更時間</param>
    /// <returns>是否過期</returns>
    bool IsPasswordExpired(DateTime lastPasswordChange);

    /// <summary>
    /// 檢查新密碼是否與目前密碼相同
    /// </summary>
    /// <param name="newPassword">新密碼</param>
    /// <param name="currentPasswordHash">目前密碼的雜湊值</param>
    /// <returns>是否相同</returns>
    bool IsSameAsCurrentPassword(string newPassword, string currentPasswordHash);
}

/// <summary>
/// 驗證結果
/// </summary>
public record ValidationResult(bool IsValid, List<string> Errors)
{
    public static ValidationResult Success() => new(true, new List<string>());
    public static ValidationResult Failure(params string[] errors) => new(false, errors.ToList());
    public static ValidationResult Failure(List<string> errors) => new(false, errors);
}
