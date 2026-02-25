using WebCMS.Core.DTOs.Article;
using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 文章服務介面
/// </summary>
public interface IArticleService
{
    /// <summary>
    /// 取得文章列表（分頁）
    /// </summary>
    Task<PagedResult<ArticleDto>> GetArticlesAsync(QueryParameters query, int? categoryId = null, string? languageCode = null);

    /// <summary>
    /// 取得單一文章
    /// </summary>
    Task<ArticleDto?> GetArticleByIdAsync(int id, string? languageCode = null);

    /// <summary>
    /// 建立文章
    /// </summary>
    Task<ArticleDto> CreateArticleAsync(CreateArticleRequest request, int? userId = null);

    /// <summary>
    /// 更新文章
    /// </summary>
    Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleRequest request, int? userId = null);

    /// <summary>
    /// 軟刪除文章
    /// </summary>
    Task<bool> SoftDeleteArticleAsync(int id);

    /// <summary>
    /// 永久刪除文章（僅限超級管理員）
    /// </summary>
    Task<bool> HardDeleteArticleAsync(int id);

    /// <summary>
    /// 檢查網址代稱是否已存在
    /// </summary>
    Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null);

    /// <summary>
    /// 驗證文章標題（最多 200 字元）
    /// </summary>
    bool ValidateArticleTitle(string title);

    /// <summary>
    /// 取得所有標籤
    /// </summary>
    Task<List<string>> GetAllTagsAsync();

    /// <summary>
    /// 依分類取得文章數量
    /// </summary>
    Task<int> GetArticleCountByCategoryAsync(int categoryId);
}
