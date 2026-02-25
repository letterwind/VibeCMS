using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 語言檔服務介面
/// 用於取得和管理語言檔
/// 支持從資料庫或靜態檔案載入語言資源
/// </summary>
public interface ILanguageFileService
{
    /// <summary>
    /// 取得語言檔
    /// 優先級：資料庫 → 靜態資源 (wwwroot/assets/i18n/{lang}.json)
    /// </summary>
    Task<LanguageFileExportDto> GetLanguageFileAsync(string languageCode);

    /// <summary>
    /// 保存語言檔到資料庫
    /// </summary>
    Task SaveLanguageFileAsync(string languageCode, LanguageFileExportDto fileData);

    /// <summary>
    /// 驗證語言檔格式
    /// </summary>
    (bool IsValid, string? ErrorMessage) ValidateLanguageFile(string fileContent);

    /// <summary>
    /// 取得語言檔版本歷史（可選功能）
    /// </summary>
    Task<List<(DateTime Timestamp, string Version)>> GetLanguageFileHistoryAsync(string languageCode);

    /// <summary>
    /// 清除快取
    /// </summary>
    Task ClearCacheAsync(string languageCode);
}
