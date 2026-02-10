namespace WebCMS.Core.DTOs.Function;

/// <summary>
/// 功能 DTO
/// </summary>
public record FunctionDto(
    int Id,
    string Name,
    string Code,
    string? Url,
    bool OpenInNewWindow,
    string? Icon,
    int? ParentId,
    int SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<FunctionDto>? Children = null
);
