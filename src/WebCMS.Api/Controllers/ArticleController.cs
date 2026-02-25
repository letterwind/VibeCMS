using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Api.Authorization;
using WebCMS.Core.DTOs.Article;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Extensions;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 文章管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticleController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ArticleController(IArticleService articleService, IHttpContextAccessor httpContextAccessor)
    {
        _articleService = articleService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 取得文章列表（分頁）
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ArticleDto>>> GetArticles(
        [FromQuery] QueryParameters query,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? lang = null)
    {
        // 取得語言代碼（優先級：查詢參數 > Header > Default）
        var languageCode = lang ?? _httpContextAccessor.GetLanguageCodeFromContext();
        
        var result = await _articleService.GetArticlesAsync(query, categoryId, languageCode);
        return Ok(result);
    }

    /// <summary>
    /// 取得單一文章
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ArticleDto>> GetArticle(int id, [FromQuery] string? lang = null)
    {
        var languageCode = lang ?? _httpContextAccessor.GetLanguageCodeFromContext();
        
        var article = await _articleService.GetArticleByIdAsync(id, languageCode);
        if (article == null)
        {
            return NotFound(new { message = "找不到指定的文章" });
        }
        return Ok(article);
    }

    /// <summary>
    /// 建立文章
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] CreateArticleRequest request)
    {
        // 檢查網址代稱是否已存在
        if (await _articleService.IsSlugExistsAsync(request.Slug))
        {
            return BadRequest(new { message = "網址代稱已存在" });
        }

        // 驗證標題長度
        if (!_articleService.ValidateArticleTitle(request.Title))
        {
            return BadRequest(new { message = "文章標題最多 200 字元" });
        }

        try
        {
            // 取得當前使用者 ID（如果有的話）
            int? userId = null;
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (int.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var article = await _articleService.CreateArticleAsync(request, userId);
            return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新文章
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ArticleDto>> UpdateArticle(int id, [FromBody] UpdateArticleRequest request)
    {
        // 檢查網址代稱是否已被其他文章使用
        if (await _articleService.IsSlugExistsAsync(request.Slug, id))
        {
            return BadRequest(new { message = "網址代稱已存在" });
        }

        // 驗證標題長度
        if (!_articleService.ValidateArticleTitle(request.Title))
        {
            return BadRequest(new { message = "文章標題最多 200 字元" });
        }

        try
        {
            // 取得當前使用者 ID（如果有的話）
            int? userId = null;
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (int.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var article = await _articleService.UpdateArticleAsync(id, request, userId);
            if (article == null)
            {
                return NotFound(new { message = "找不到指定的文章" });
            }
            return Ok(article);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除文章（軟刪除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteArticle(int id)
    {
        var result = await _articleService.SoftDeleteArticleAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的文章" });
        }
        return NoContent();
    }

    /// <summary>
    /// 永久刪除文章（僅限超級管理員）
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [SuperAdmin]
    public async Task<ActionResult> HardDeleteArticle(int id)
    {
        var result = await _articleService.HardDeleteArticleAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的文章" });
        }
        return NoContent();
    }

    /// <summary>
    /// 取得所有標籤
    /// </summary>
    [HttpGet("tags")]
    public async Task<ActionResult<List<string>>> GetAllTags()
    {
        var tags = await _articleService.GetAllTagsAsync();
        return Ok(tags);
    }
}
