using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Settings;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 設定服務實作
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;
    private readonly string _faviconFolder = "favicons";

    public SettingsService(ApplicationDbContext context, string uploadPath = "wwwroot/uploads")
    {
        _context = context;
        _uploadPath = uploadPath;
    }

    /// <summary>
    /// 單一記錄限制標記
    /// </summary>
    public bool IsSingleRecordOnly => true;

    #region 網站設定

    public async Task<SiteSettingsDto> GetSiteSettingsAsync()
    {
        await EnsureSettingsExistAsync();
        var settings = await _context.SiteSettings.FirstAsync();
        return MapToSiteSettingsDto(settings);
    }

    public async Task<SiteSettingsDto> UpdateSiteSettingsAsync(UpdateSiteSettingsRequest request)
    {
        await EnsureSettingsExistAsync();
        var settings = await _context.SiteSettings.FirstAsync();

        settings.MetaTitle = request.MetaTitle;
        settings.MetaDescription = request.MetaDescription;
        settings.MetaKeywords = request.MetaKeywords;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToSiteSettingsDto(settings);
    }

    public async Task<string> UploadFaviconAsync(string fileName, Stream fileStream)
    {
        await EnsureSettingsExistAsync();

        // 確保上傳目錄存在
        var faviconPath = Path.Combine(_uploadPath, _faviconFolder);
        if (!Directory.Exists(faviconPath))
        {
            Directory.CreateDirectory(faviconPath);
        }

        // 產生唯一檔名
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"favicon_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var fullPath = Path.Combine(faviconPath, uniqueFileName);

        // 儲存檔案
        using (var file = File.Create(fullPath))
        {
            await fileStream.CopyToAsync(file);
        }

        // 更新資料庫
        var settings = await _context.SiteSettings.FirstAsync();
        var relativePath = $"/uploads/{_faviconFolder}/{uniqueFileName}";
        settings.FaviconPath = relativePath;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return relativePath;
    }

    #endregion

    #region 頁首設定

    public async Task<HtmlSettingsDto> GetHeaderSettingsAsync()
    {
        await EnsureSettingsExistAsync();
        var settings = await _context.HeaderSettings.FirstAsync();
        return MapToHtmlSettingsDto(settings.Id, settings.HtmlContent, settings.UpdatedAt);
    }

    public async Task<HtmlSettingsDto> UpdateHeaderSettingsAsync(UpdateHtmlSettingsRequest request)
    {
        await EnsureSettingsExistAsync();
        var settings = await _context.HeaderSettings.FirstAsync();

        settings.HtmlContent = request.HtmlContent;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToHtmlSettingsDto(settings.Id, settings.HtmlContent, settings.UpdatedAt);
    }

    #endregion

    #region 頁尾設定

    public async Task<HtmlSettingsDto> GetFooterSettingsAsync()
    {
        await EnsureSettingsExistAsync();
        var settings = await _context.FooterSettings.FirstAsync();
        return MapToHtmlSettingsDto(settings.Id, settings.HtmlContent, settings.UpdatedAt);
    }

    public async Task<HtmlSettingsDto> UpdateFooterSettingsAsync(UpdateHtmlSettingsRequest request)
    {
        await EnsureSettingsExistAsync();
        var settings = await _context.FooterSettings.FirstAsync();

        settings.HtmlContent = request.HtmlContent;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToHtmlSettingsDto(settings.Id, settings.HtmlContent, settings.UpdatedAt);
    }

    #endregion

    #region 單一記錄管理

    public async Task EnsureSettingsExistAsync()
    {
        // 確保網站設定存在
        if (!await _context.SiteSettings.AnyAsync())
        {
            _context.SiteSettings.Add(new SiteSettings
            {
                MetaTitle = "WebCMS",
                MetaDescription = "Web Content Management System",
                MetaKeywords = "CMS, Web, Content",
                UpdatedAt = DateTime.UtcNow
            });
        }

        // 確保頁首設定存在
        if (!await _context.HeaderSettings.AnyAsync())
        {
            _context.HeaderSettings.Add(new HeaderSettings
            {
                HtmlContent = "",
                UpdatedAt = DateTime.UtcNow
            });
        }

        // 確保頁尾設定存在
        if (!await _context.FooterSettings.AnyAsync())
        {
            _context.FooterSettings.Add(new FooterSettings
            {
                HtmlContent = "",
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region 映射方法

    private static SiteSettingsDto MapToSiteSettingsDto(SiteSettings settings)
    {
        return new SiteSettingsDto(
            settings.Id,
            settings.MetaTitle,
            settings.MetaDescription,
            settings.MetaKeywords,
            settings.FaviconPath,
            settings.FaviconPath, // FaviconUrl 與 FaviconPath 相同
            settings.UpdatedAt
        );
    }

    private static HtmlSettingsDto MapToHtmlSettingsDto(int id, string? htmlContent, DateTime updatedAt)
    {
        return new HtmlSettingsDto(id, htmlContent, updatedAt);
    }

    #endregion
}
