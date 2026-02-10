using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.Function;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 功能服務介面
/// </summary>
public interface IFunctionService
{
    /// <summary>
    /// 取得功能列表（分頁）
    /// </summary>
    Task<PagedResult<FunctionDto>> GetFunctionsAsync(QueryParameters query);

    /// <summary>
    /// 取得單一功能
    /// </summary>
    Task<FunctionDto?> GetFunctionByIdAsync(int id);

    /// <summary>
    /// 建立功能
    /// </summary>
    Task<FunctionDto> CreateFunctionAsync(CreateFunctionRequest request);

    /// <summary>
    /// 更新功能
    /// </summary>
    Task<FunctionDto?> UpdateFunctionAsync(int id, UpdateFunctionRequest request);

    /// <summary>
    /// 軟刪除功能
    /// </summary>
    Task<bool> SoftDeleteFunctionAsync(int id);

    /// <summary>
    /// 永久刪除功能（僅限超級管理員）
    /// </summary>
    Task<bool> HardDeleteFunctionAsync(int id);

    /// <summary>
    /// 檢查功能代碼是否已存在
    /// </summary>
    Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// 取得選單樹狀結構
    /// </summary>
    Task<List<FunctionDto>> GetMenuTreeAsync();

    /// <summary>
    /// 取得所有功能（不分頁，用於下拉選單）
    /// </summary>
    Task<List<FunctionDto>> GetAllFunctionsAsync();
}
