namespace WebCMS.Core.Validators;

/// <summary>
/// 密碼驗證上下文
/// 用於傳遞密碼和帳號以進行驗證
/// </summary>
public record PasswordValidationContext(string Password, string Account);
