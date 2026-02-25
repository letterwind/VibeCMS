using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 語言資源服務介面
/// 用於管理系統介面文字的多語言翻譯
/// </summary>
public interface ILanguageResourceService
{
    /// <summary>
    /// 取得指定語言的所有資源
    /// </summary>
    Task<List<LanguageResourceDto>> GetResourcesByLanguageAsync(string languageCode);

    /// <summary>
    /// 取得指定語言的單一資源
    /// </summary>
    Task<LanguageResourceDto?> GetResourceByKeyAsync(string languageCode, string resourceKey);

    /// <summary>
    /// 建立語言資源
    /// </summary>
    Task<LanguageResourceDto> CreateResourceAsync(CreateOrUpdateLanguageResourceRequest request);

    /// <summary>
    /// 批量建立或更新語言資源
    /// </summary>
    Task<List<LanguageResourceDto>> BatchCreateOrUpdateResourcesAsync(BatchUpdateLanguageResourcesRequest request);

    /// <summary>
    /// 更新語言資源
    /// </summary>
    Task<LanguageResourceDto> UpdateResourceAsync(int id, CreateOrUpdateLanguageResourceRequest request);

    /// <summary>
    /// 刪除語言資源
    /// </summary>
    Task<bool> DeleteResourceAsync(int id);

    /// <summary>
    /// 刪除指定語言的所有資源
    /// </summary>
    Task<int> DeleteLanguageResourcesAsync(string languageCode);

    /// <summary>
    /// 匯出指定語言的資源為 JSON
    /// </summary>
    Task<LanguageFileExportDto> ExportResourcesAsync(string languageCode);

    /// <summary>
    /// 從 JSON 匯入資源
    /// </summary>
    Task<int> ImportResourcesAsync(string languageCode, string fileContent, bool overwrite = false);

    /// <summary>
    /// 驗證 JSON 格式
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateLanguageFileAsync(string fileContent);

    /// <summary>
    /// 清除快取
    /// </summary>
    Task ClearCacheAsync(string languageCode);
}
