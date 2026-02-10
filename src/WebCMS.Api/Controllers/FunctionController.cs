using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Api.Authorization;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.Function;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 功能管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FunctionController : ControllerBase
{
    private readonly IFunctionService _functionService;

    public FunctionController(IFunctionService functionService)
    {
        _functionService = functionService;
    }

    /// <summary>
    /// 取得功能列表（分頁）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<FunctionDto>>> GetFunctions([FromQuery] QueryParameters query)
    {
        var result = await _functionService.GetFunctionsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得所有功能（不分頁，用於下拉選單）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<FunctionDto>>> GetAllFunctions()
    {
        var result = await _functionService.GetAllFunctionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// 取得選單樹狀結構
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<FunctionDto>>> GetMenuTree()
    {
        var result = await _functionService.GetMenuTreeAsync();
        return Ok(result);
    }


    /// <summary>
    /// 取得單一功能
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FunctionDto>> GetFunction(int id)
    {
        var function = await _functionService.GetFunctionByIdAsync(id);
        if (function == null)
        {
            return NotFound(new { message = "找不到指定的功能" });
        }
        return Ok(function);
    }

    /// <summary>
    /// 建立功能
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FunctionDto>> CreateFunction([FromBody] CreateFunctionRequest request)
    {
        // 檢查功能代碼是否已存在
        if (await _functionService.IsCodeExistsAsync(request.Code))
        {
            return BadRequest(new { message = "功能代碼已存在" });
        }

        var function = await _functionService.CreateFunctionAsync(request);
        return CreatedAtAction(nameof(GetFunction), new { id = function.Id }, function);
    }

    /// <summary>
    /// 更新功能
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FunctionDto>> UpdateFunction(int id, [FromBody] UpdateFunctionRequest request)
    {
        // 檢查功能代碼是否已被其他功能使用
        if (await _functionService.IsCodeExistsAsync(request.Code, id))
        {
            return BadRequest(new { message = "功能代碼已存在" });
        }

        var function = await _functionService.UpdateFunctionAsync(id, request);
        if (function == null)
        {
            return NotFound(new { message = "找不到指定的功能" });
        }
        return Ok(function);
    }

    /// <summary>
    /// 刪除功能（軟刪除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFunction(int id)
    {
        var result = await _functionService.SoftDeleteFunctionAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的功能" });
        }
        return NoContent();
    }

    /// <summary>
    /// 永久刪除功能（僅限超級管理員）
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [SuperAdmin]
    public async Task<ActionResult> HardDeleteFunction(int id)
    {
        var result = await _functionService.HardDeleteFunctionAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的功能" });
        }
        return NoContent();
    }
}
