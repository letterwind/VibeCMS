using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 語言管理 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LanguageController : ControllerBase
{
    private readonly ILanguageService _languageService;

    public LanguageController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    /// <summary>
    /// 取得所有啟用的語言
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<LanguageDto>>> GetLanguages()
    {
        var languages = await _languageService.GetActiveLanguagesAsync();
        var languageDtos = languages.Select(l => new LanguageDto(
            l.Id,
            l.LanguageCode,
            l.LanguageName,
            l.IsActive,
            l.SortOrder
        ));

        return Ok(languageDtos);
    }

    /// <summary>
    /// 取得單一語言
    /// </summary>
    [HttpGet("{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<LanguageDto>> GetLanguage(string code)
    {
        var language = await _languageService.GetLanguageByCodeAsync(code);
        if (language == null)
        {
            return NotFound(new { message = $"語言 '{code}' 不存在" });
        }

        return Ok(new LanguageDto(
            language.Id,
            language.LanguageCode,
            language.LanguageName,
            language.IsActive,
            language.SortOrder
        ));
    }

    /// <summary>
    /// 驗證語言代碼是否有效
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> ValidateLanguage([FromBody] string languageCode)
    {
        var isValid = await _languageService.IsValidLanguageCodeAsync(languageCode);
        return Ok(isValid);
    }
}
