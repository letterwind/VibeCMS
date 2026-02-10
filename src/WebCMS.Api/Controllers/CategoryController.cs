using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Api.Authorization;
using WebCMS.Core.DTOs.Category;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 文章分類管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// 取得分類列表（分頁）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetCategories([FromQuery] QueryParameters query)
    {
        var result = await _categoryService.GetCategoriesAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得所有分類（不分頁，用於下拉選單）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return Ok(result);
    }

    /// <summary>
    /// 取得分類樹狀結構
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<CategoryTreeDto>>> GetCategoryTree()
    {
        var result = await _categoryService.GetCategoryTreeAsync();
        return Ok(result);
    }

    /// <summary>
    /// 取得單一分類
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "找不到指定的分類" });
        }
        return Ok(category);
    }

    /// <summary>
    /// 建立分類
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        // 檢查網址代稱是否已存在
        if (await _categoryService.IsSlugExistsAsync(request.Slug))
        {
            return BadRequest(new { message = "網址代稱已存在" });
        }

        // 檢查是否可以在指定父分類下新增子分類
        if (!await _categoryService.CanAddChildCategoryAsync(request.ParentId))
        {
            return BadRequest(new { message = "分類層級已達上限（最多 3 層）" });
        }

        // 驗證分類名稱長度
        if (!_categoryService.ValidateCategoryName(request.Name))
        {
            return BadRequest(new { message = "分類名稱最多 20 字元" });
        }

        try
        {
            var category = await _categoryService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新分類
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        // 檢查網址代稱是否已被其他分類使用
        if (await _categoryService.IsSlugExistsAsync(request.Slug, id))
        {
            return BadRequest(new { message = "網址代稱已存在" });
        }

        // 驗證分類名稱長度
        if (!_categoryService.ValidateCategoryName(request.Name))
        {
            return BadRequest(new { message = "分類名稱最多 20 字元" });
        }

        try
        {
            var category = await _categoryService.UpdateCategoryAsync(id, request);
            if (category == null)
            {
                return NotFound(new { message = "找不到指定的分類" });
            }
            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除分類（軟刪除，含級聯處理子分類）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.SoftDeleteCategoryAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的分類" });
        }
        return NoContent();
    }

    /// <summary>
    /// 永久刪除分類（僅限超級管理員）
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [SuperAdmin]
    public async Task<ActionResult> HardDeleteCategory(int id)
    {
        var result = await _categoryService.HardDeleteCategoryAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的分類" });
        }
        return NoContent();
    }

    /// <summary>
    /// 檢查是否可以在指定父分類下新增子分類
    /// </summary>
    [HttpGet("can-add-child/{parentId?}")]
    public async Task<ActionResult<bool>> CanAddChildCategory(int? parentId)
    {
        var result = await _categoryService.CanAddChildCategoryAsync(parentId);
        return Ok(new { canAdd = result });
    }
}
