using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.User;

/// <summary>
/// 建立使用者請求 DTO
/// </summary>
public record CreateUserRequest(
    [Required(ErrorMessage = "帳號為必填欄位")]
    string Account,

    [Required(ErrorMessage = "密碼為必填欄位")]
    string Password,

    [Required(ErrorMessage = "顯示名稱為必填欄位")]
    [MaxLength(100, ErrorMessage = "顯示名稱最多 100 字元")]
    string DisplayName,

    List<int>? RoleIds
);
