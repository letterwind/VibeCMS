using WebCMS.Core.Entities;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 語言服務介面
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// 取得所有啟用的語言
    /// </summary>
    Task<IEnumerable<Language>> GetActiveLanguagesAsync();
    
    /// <summary>
    /// 取得單一語言
    /// </summary>
    Task<Language?> GetLanguageByCodeAsync(string languageCode);
    
    /// <summary>
    /// 驗證語言代碼是否有效
    /// </summary>
    Task<bool> IsValidLanguageCodeAsync(string languageCode);
    
    /// <summary>
    /// 快取清除
    /// </summary>
    void ClearCache();
}
