using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.Auth;

/// <summary>
/// 登入請求 DTO
/// </summary>
public record LoginRequest(
    [Required(ErrorMessage = "帳號為必填欄位")]
    string Account,

    [Required(ErrorMessage = "密碼為必填欄位")]
    string Password,

    [Required(ErrorMessage = "驗證碼為必填欄位")]
    string Captcha,

    [Required(ErrorMessage = "驗證碼 Token 為必填欄位")]
    string CaptchaToken
);
