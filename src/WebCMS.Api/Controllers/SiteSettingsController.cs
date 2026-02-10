using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.DTOs.Settings;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 網站設定控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SiteSettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SiteSettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// 取得網站設定
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SiteSettingsDto>> GetSiteSettings()
    {
        var settings = await _settingsService.GetSiteSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// 更新網站設定
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<SiteSettingsDto>> UpdateSiteSettings([FromBody] UpdateSiteSettingsRequest request)
    {
        var settings = await _settingsService.UpdateSiteSettingsAsync(request);
        return Ok(settings);
    }

    /// <summary>
    /// 上傳 Favicon
    /// </summary>
    [HttpPost("favicon")]
    public async Task<ActionResult<object>> UploadFavicon(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "請選擇要上傳的檔案" });
        }

        // 驗證檔案類型
        var allowedExtensions = new[] { ".ico", ".png", ".jpg", ".jpeg", ".gif", ".svg" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "不支援的檔案格式，請上傳 ICO、PNG、JPG、GIF 或 SVG 格式的圖片" });
        }

        // 驗證檔案大小（最大 1MB）
        if (file.Length > 1024 * 1024)
        {
            return BadRequest(new { message = "檔案大小不得超過 1MB" });
        }

        using var stream = file.OpenReadStream();
        var faviconPath = await _settingsService.UploadFaviconAsync(file.FileName, stream);

        return Ok(new { faviconPath });
    }
}
