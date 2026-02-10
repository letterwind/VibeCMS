using WebCMS.Core.DTOs.Role;

namespace WebCMS.Core.DTOs.User;

/// <summary>
/// 使用者 DTO
/// </summary>
public record UserDto(
    int Id,
    string Account,
    string DisplayName,
    List<RoleDto> Roles,
    bool IsPasswordExpired,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
