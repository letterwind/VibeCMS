using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.Role;

/// <summary>
/// 更新角色請求 DTO
/// </summary>
public record UpdateRoleRequest(
    [Required(ErrorMessage = "角色名稱為必填欄位")]
    [MaxLength(50, ErrorMessage = "角色名稱最多 50 字元")]
    string Name,

    [MaxLength(200, ErrorMessage = "角色描述最多 200 字元")]
    string? Description,

    [Required(ErrorMessage = "階層等級為必填欄位")]
    [Range(1, int.MaxValue, ErrorMessage = "階層等級必須大於 0")]
    int HierarchyLevel
);
