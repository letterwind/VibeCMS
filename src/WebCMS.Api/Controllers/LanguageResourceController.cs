using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguageResourceController : ControllerBase
{
    private readonly ILanguageResourceService _languageResourceService;

    public LanguageResourceController(ILanguageResourceService languageResourceService)
    {
        _languageResourceService = languageResourceService;
    }

    /// <summary>
    /// 取得指定語言的所有資源
    /// </summary>
    [HttpGet("{languageCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetResourcesByLanguage(string languageCode)
    {
        var resources = await _languageResourceService.GetResourcesByLanguageAsync(languageCode);
        return Ok(new { success = true, data = resources });
    }

    /// <summary>
    /// 取得指定語言的單一資源
    /// </summary>
    [HttpGet("{languageCode}/{resourceKey}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetResourceByKey(string languageCode, string resourceKey)
    {
        var resource = await _languageResourceService.GetResourceByKeyAsync(languageCode, resourceKey);
        if (resource == null)
        {
            return NotFound(new { success = false, message = "Resource not found" });
        }

        return Ok(new { success = true, data = resource });
    }

    /// <summary>
    /// 建立語言資源
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateResource([FromBody] CreateOrUpdateLanguageResourceRequest request)
    {
        try
        {
            var resource = await _languageResourceService.CreateResourceAsync(request);
            return CreatedAtAction(nameof(GetResourceByKey), new { languageCode = request.LanguageCode, resourceKey = request.ResourceKey }, new { success = true, data = resource });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 批量建立或更新語言資源
    /// </summary>
    [HttpPost("batch-update")]
    public async Task<IActionResult> BatchCreateOrUpdateResources([FromBody] BatchUpdateLanguageResourcesRequest request)
    {
        try
        {
            var resources = await _languageResourceService.BatchCreateOrUpdateResourcesAsync(request);
            return Ok(new { success = true, data = resources, count = resources.Count });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 更新語言資源
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResource(int id, [FromBody] CreateOrUpdateLanguageResourceRequest request)
    {
        try
        {
            var resource = await _languageResourceService.UpdateResourceAsync(id, request);
            return Ok(new { success = true, data = resource });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除語言資源
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var result = await _languageResourceService.DeleteResourceAsync(id);
        if (!result)
        {
            return NotFound(new { success = false, message = "Resource not found" });
        }

        return Ok(new { success = true, message = "Resource deleted successfully" });
    }

    /// <summary>
    /// 刪除指定語言的所有資源
    /// </summary>
    [HttpDelete("language/{languageCode}")]
    public async Task<IActionResult> DeleteLanguageResources(string languageCode)
    {
        try
        {
            var count = await _languageResourceService.DeleteLanguageResourcesAsync(languageCode);
            return Ok(new { success = true, message = $"Deleted {count} resources" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 匯出指定語言的資源為 JSON
    /// </summary>
    [HttpGet("{languageCode}/export")]
    [AllowAnonymous]
    public async Task<IActionResult> ExportResources(string languageCode)
    {
        try
        {
            var exportData = await _languageResourceService.ExportResourcesAsync(languageCode);
            return Ok(new { success = true, data = exportData });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 從 JSON 匯入資源
    /// </summary>
    [HttpPost("{languageCode}/import")]
    public async Task<IActionResult> ImportResources(string languageCode, [FromBody] LanguageFileImportRequest request)
    {
        try
        {
            request.LanguageCode = languageCode;
            var count = await _languageResourceService.ImportResourcesAsync(languageCode, request.FileContent, request.Overwrite);
            return Ok(new { success = true, message = $"Imported {count} resources" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 驗證語言檔格式
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateLanguageFile([FromBody] dynamic request)
    {
        try
        {
            var fileContent = request?.fileContent?.ToString() ?? "";
            var result = await _languageResourceService.ValidateLanguageFileAsync(fileContent);

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
    /// 清除快取
    /// </summary>
    [HttpPost("{languageCode}/clear-cache")]
    public async Task<IActionResult> ClearCache(string languageCode)
    {
        try
        {
            await _languageResourceService.ClearCacheAsync(languageCode);
            return Ok(new { success = true, message = "Cache cleared successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
