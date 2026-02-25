using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.Article;

/// <summary>
/// 更新文章請求 DTO
/// </summary>
public record UpdateArticleRequest(
    [Required(ErrorMessage = "文章標題為必填欄位")]
    [MaxLength(200, ErrorMessage = "文章標題最多 200 字元")]
    string Title,

    [Required(ErrorMessage = "文章內容為必填欄位")]
    string Content,

    [Required(ErrorMessage = "網址代稱為必填欄位")]
    [MaxLength(200, ErrorMessage = "網址代稱最多 200 字元")]
    string Slug,

    [Required(ErrorMessage = "分類為必填欄位")]
    int CategoryId,

    [MaxLength(10, ErrorMessage = "語言代碼最多 10 字元")]
    string? LanguageCode,

    List<string>? Tags,

    [MaxLength(100, ErrorMessage = "SEO 標題最多 100 字元")]
    string? MetaTitle,

    [MaxLength(200, ErrorMessage = "SEO 描述最多 200 字元")]
    string? MetaDescription,

    [MaxLength(200, ErrorMessage = "SEO 關鍵字最多 200 字元")]
    string? MetaKeywords
);
