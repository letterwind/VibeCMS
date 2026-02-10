using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.Auth;

/// <summary>
/// 變更密碼請求 DTO
/// </summary>
public record ChangePasswordRequest(
    [Required(ErrorMessage = "目前密碼為必填欄位")]
    string CurrentPassword,

    [Required(ErrorMessage = "新密碼為必填欄位")]
    string NewPassword
);
