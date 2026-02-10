namespace WebCMS.Core.DTOs.Role;

/// <summary>
/// 角色 DTO
/// </summary>
public record RoleDto(
    int Id,
    string Name,
    string? Description,
    int HierarchyLevel,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
