using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.DTOs.Article;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Extensions;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 文章翻譯 API 控制器
/// 提供多語言文章操作端點
/// </summary>
[ApiController]
[Route("api/articles/{id}/translations")]
[Authorize]
public class ArticleTranslationController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly ITranslationService<Article> _translationService;
    private readonly ILanguageService _languageService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ArticleTranslationController(
        IArticleService articleService,
        ITranslationService<Article> translationService,
        ILanguageService languageService,
        IHttpContextAccessor httpContextAccessor)
    {
        _articleService = articleService;
        _translationService = translationService;
        _languageService = languageService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 取得特定文章的所有語言版本
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAllTranslations(int id)
    {
        var translations = await _translationService.GetAllLanguageVersionsAsync(id);
        if (!translations.Any())
        {
            return NotFound(new { message = "文章不存在或無任何版本" });
        }

        var dtos = new List<ArticleDto>();
        foreach (var translation in translations)
        {
            var dto = await _articleService.GetArticleByIdAsync(id, translation.LanguageCode);
            if (dto != null)
            {
                dtos.Add(dto);
            }
        }

        return Ok(dtos);
    }

    /// <summary>
    /// 取得文章的翻譯狀態
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult<TranslationStatusDto>> GetTranslationStatus(int id)
    {
        var status = await _translationService.GetTranslationStatusAsync(id);
        if (!status.Any())
        {
            return NotFound(new { message = "文章不存在" });
        }

        // 計算完成百分率
        var completedCount = status.Values.Count(v => v);
        var completionPercentage = status.Any() ? (completedCount * 100) / status.Count : 0;

        return Ok(new TranslationStatusDto(
            id,
            "Article",
            new Dictionary<string, bool>(status),
            completionPercentage
        ));
    }

    /// <summary>
    /// 複製文章翻譯（從一種語言到另一種語言）
    /// </summary>
    [HttpPost("copy")]
    public async Task<ActionResult<ArticleDto>> CopyTranslation(
        int id,
        [FromQuery] string sourceLanguage,
        [FromQuery] string targetLanguage)
    {
        // 驗證語言
        if (!await _languageService.IsValidLanguageCodeAsync(sourceLanguage))
        {
            return BadRequest(new { message = $"源語言 '{sourceLanguage}' 無效" });
        }

        if (!await _languageService.IsValidLanguageCodeAsync(targetLanguage))
        {
            return BadRequest(new { message = $"目標語言 '{targetLanguage}' 無效" });
        }

        // 複製翻譯
        var copied = await _translationService.CopyTranslationAsync(id, sourceLanguage, targetLanguage);
        if (copied == null)
        {
            return NotFound(new { message = "源語言版本不存在" });
        }

        var dto = await _articleService.GetArticleByIdAsync(id, targetLanguage);
        return Ok(dto);
    }

    /// <summary>
    /// 刪除特定語言版本的文章
    /// </summary>
    [HttpDelete("{language}")]
    public async Task<IActionResult> DeleteLanguageVersion(int id, string language)
    {
        var deleted = await _translationService.DeleteLanguageVersionAsync(id, language);
        if (!deleted)
        {
            return NotFound(new { message = "文章版本不存在" });
        }

        return Ok(new { message = $"已刪除語言 '{language}' 的版本" });
    }
}
