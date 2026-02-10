using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.DTOs.Settings;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 頁首設定控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HeaderSettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public HeaderSettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// 取得頁首設定
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<HtmlSettingsDto>> GetHeaderSettings()
    {
        var settings = await _settingsService.GetHeaderSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// 更新頁首設定
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<HtmlSettingsDto>> UpdateHeaderSettings([FromBody] UpdateHtmlSettingsRequest request)
    {
        var settings = await _settingsService.UpdateHeaderSettingsAsync(request);
        return Ok(settings);
    }
}
