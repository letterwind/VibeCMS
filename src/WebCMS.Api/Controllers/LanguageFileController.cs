using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguageFileController : ControllerBase
{
    private readonly ILanguageFileService _languageFileService;

    public LanguageFileController(ILanguageFileService languageFileService)
    {
        _languageFileService = languageFileService;
    }

    /// <summary>
    /// 取得語言檔
    /// 返回 JSON 格式的語言資源檔
    /// URL: /api/i18n/zh-TW.json
    /// </summary>
    [HttpGet("{languageCode}.json")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLanguageFile(string languageCode)
    {
        try
        {
            var languageFile = await _languageFileService.GetLanguageFileAsync(languageCode);

            // 直接返回資源字典作為 JSON，前端可直接用作語言檔
            return Ok(languageFile.Resources);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 取得完整的語言檔資訊（包含後設資料）
    /// </summary>
    [HttpGet("{languageCode}/full")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFullLanguageFile(string languageCode)
    {
        try
        {
            var languageFile = await _languageFileService.GetLanguageFileAsync(languageCode);
            return Ok(new { success = true, data = languageFile });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 驗證語言檔格式
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public IActionResult ValidateLanguageFile([FromBody] dynamic request)
    {
        try
        {
            var fileContent = request?.fileContent?.ToString() ?? "";
            var result = _languageFileService.ValidateLanguageFile(fileContent);

            if (!result.IsValid)
            {
                return BadRequest(new { success = false, message = result.ErrorMessage });
            }

            return Ok(new { success = true, message = "Language file is valid" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 清除語言檔快取
    /// </summary>
    [HttpPost("{languageCode}/clear-cache")]
    public async Task<IActionResult> ClearCache(string languageCode)
    {
        try
        {
            await _languageFileService.ClearCacheAsync(languageCode);
            return Ok(new { success = true, message = "Cache cleared successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 取得語言檔版本歷史
    /// </summary>
    [HttpGet("{languageCode}/history")]
    public async Task<IActionResult> GetLanguageFileHistory(string languageCode)
    {
        try
        {
            var history = await _languageFileService.GetLanguageFileHistoryAsync(languageCode);
            return Ok(new { success = true, data = history });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
