using WebCMS.Core.DTOs.Category;
using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 分類服務介面
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// 取得分類列表（分頁）
    /// </summary>
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(QueryParameters query);

    /// <summary>
    /// 取得單一分類
    /// </summary>
    Task<CategoryDto?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// 建立分類
    /// </summary>
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);

    /// <summary>
    /// 更新分類
    /// </summary>
    Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryRequest request);

    /// <summary>
    /// 軟刪除分類（含級聯處理子分類）
    /// </summary>
    Task<bool> SoftDeleteCategoryAsync(int id);

    /// <summary>
    /// 永久刪除分類（僅限超級管理員）
    /// </summary>
    Task<bool> HardDeleteCategoryAsync(int id);

    /// <summary>
    /// 檢查網址代稱是否已存在
    /// </summary>
    Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null);

    /// <summary>
    /// 取得分類樹狀結構
    /// </summary>
    Task<List<CategoryTreeDto>> GetCategoryTreeAsync();

    /// <summary>
    /// 取得所有分類（不分頁，用於下拉選單）
    /// </summary>
    Task<List<CategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// 取得分類深度（層級）
    /// </summary>
    Task<int> GetCategoryDepthAsync(int categoryId);

    /// <summary>
    /// 檢查是否可以在指定父分類下新增子分類（最多 3 層限制）
    /// </summary>
    Task<bool> CanAddChildCategoryAsync(int? parentId);

    /// <summary>
    /// 驗證分類名稱（最多 20 字元）
    /// </summary>
    bool ValidateCategoryName(string name);
}
