namespace WebCMS.Core.DTOs.Category;

/// <summary>
/// 分類樹狀結構 DTO
/// </summary>
public record CategoryTreeDto(
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
    DateTime UpdatedAt,
    List<CategoryTreeDto>? Children = null
);
