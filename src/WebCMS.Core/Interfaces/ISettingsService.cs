using WebCMS.Core.DTOs.Settings;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 設定服務介面
/// </summary>
public interface ISettingsService
{
    #region 網站設定

    /// <summary>
    /// 取得網站設定
    /// </summary>
    Task<SiteSettingsDto> GetSiteSettingsAsync();

    /// <summary>
    /// 更新網站設定
    /// </summary>
    Task<SiteSettingsDto> UpdateSiteSettingsAsync(UpdateSiteSettingsRequest request);

    /// <summary>
    /// 上傳 Favicon
    /// </summary>
    /// <param name="fileName">檔案名稱</param>
    /// <param name="fileStream">檔案串流</param>
    /// <returns>Favicon 路徑</returns>
    Task<string> UploadFaviconAsync(string fileName, Stream fileStream);

    #endregion

    #region 頁首設定

    /// <summary>
    /// 取得頁首設定
    /// </summary>
    Task<HtmlSettingsDto> GetHeaderSettingsAsync();

    /// <summary>
    /// 更新頁首設定
    /// </summary>
    Task<HtmlSettingsDto> UpdateHeaderSettingsAsync(UpdateHtmlSettingsRequest request);

    #endregion

    #region 頁尾設定

    /// <summary>
    /// 取得頁尾設定
    /// </summary>
    Task<HtmlSettingsDto> GetFooterSettingsAsync();

    /// <summary>
    /// 更新頁尾設定
    /// </summary>
    Task<HtmlSettingsDto> UpdateFooterSettingsAsync(UpdateHtmlSettingsRequest request);

    #endregion

    #region 單一記錄限制

    /// <summary>
    /// 確保設定記錄存在（如果不存在則建立預設記錄）
    /// </summary>
    Task EnsureSettingsExistAsync();

    /// <summary>
    /// 檢查是否為單一記錄（不允許新增或刪除）
    /// </summary>
    bool IsSingleRecordOnly { get; }

    #endregion
}
