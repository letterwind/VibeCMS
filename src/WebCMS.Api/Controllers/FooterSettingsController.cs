using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.DTOs.Settings;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 頁尾設定控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FooterSettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public FooterSettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// 取得頁尾設定
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<HtmlSettingsDto>> GetFooterSettings()
    {
        var settings = await _settingsService.GetFooterSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// 更新頁尾設定
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<HtmlSettingsDto>> UpdateFooterSettings([FromBody] UpdateHtmlSettingsRequest request)
    {
        var settings = await _settingsService.UpdateFooterSettingsAsync(request);
        return Ok(settings);
    }
}
