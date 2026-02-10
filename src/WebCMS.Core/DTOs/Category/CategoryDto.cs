namespace WebCMS.Core.DTOs.Category;

/// <summary>
/// 分類 DTO
/// </summary>
public record CategoryDto(
    int Id,
    string Name,
    string Slug,
    int? ParentId,
    int Level,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    int SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
