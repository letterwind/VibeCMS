using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.User;

/// <summary>
/// 更新使用者請求 DTO
/// </summary>
public record UpdateUserRequest(
    [Required(ErrorMessage = "顯示名稱為必填欄位")]
    [MaxLength(100, ErrorMessage = "顯示名稱最多 100 字元")]
    string DisplayName,

    /// <summary>
    /// 新密碼（可選，若提供則更新密碼）
    /// </summary>
    string? NewPassword,

    List<int>? RoleIds
);
