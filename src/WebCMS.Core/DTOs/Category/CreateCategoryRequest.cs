using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.Category;

/// <summary>
/// 建立分類請求 DTO
/// </summary>
public record CreateCategoryRequest(
    [Required(ErrorMessage = "分類名稱為必填欄位")]
    [MaxLength(20, ErrorMessage = "分類名稱最多 20 字元")]
    string Name,

    [Required(ErrorMessage = "網址代稱為必填欄位")]
    [MaxLength(100, ErrorMessage = "網址代稱最多 100 字元")]
    string Slug,

    int? ParentId,

    [MaxLength(100, ErrorMessage = "SEO 標題最多 100 字元")]
    string? MetaTitle,

    [MaxLength(200, ErrorMessage = "SEO 描述最多 200 字元")]
    string? MetaDescription,

    [MaxLength(200, ErrorMessage = "SEO 關鍵字最多 200 字元")]
    string? MetaKeywords,

    int SortOrder = 0
);
