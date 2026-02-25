namespace WebCMS.Core.DTOs.Role;

/// <summary>
/// 角色 DTO
/// </summary>
public record RoleDto(
    int Id,
    string Name,
    string? Description,
    string LanguageCode,
    int HierarchyLevel,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    /// <summary>
    /// 翻譯狀態（鍵為語言代碼，值為是否已翻譯）
    /// </summary>
    Dictionary<string, bool>? AvailableLanguages = null
);
