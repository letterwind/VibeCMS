using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Api.Authorization;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.Role;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 角色管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// 取得角色列表（分頁）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<RoleDto>>> GetRoles([FromQuery] QueryParameters query)
    {
        var result = await _roleService.GetRolesAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得所有角色（不分頁，用於下拉選單）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        var result = await _roleService.GetAllRolesAsync();
        return Ok(result);
    }

    /// <summary>
    /// 取得單一角色
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            return NotFound(new { message = "找不到指定的角色" });
        }
        return Ok(role);
    }

    /// <summary>
    /// 建立角色
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request)
    {
        // 檢查角色名稱是否已存在
        if (await _roleService.IsNameExistsAsync(request.Name))
        {
            return BadRequest(new { message = "角色名稱已存在" });
        }

        var role = await _roleService.CreateRoleAsync(request);
        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        // 檢查角色名稱是否已被其他角色使用
        if (await _roleService.IsNameExistsAsync(request.Name, id))
        {
            return BadRequest(new { message = "角色名稱已存在" });
        }

        var role = await _roleService.UpdateRoleAsync(id, request);
        if (role == null)
        {
            return NotFound(new { message = "找不到指定的角色" });
        }
        return Ok(role);
    }

    /// <summary>
    /// 刪除角色（軟刪除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        var result = await _roleService.SoftDeleteRoleAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的角色" });
        }
        return NoContent();
    }

    /// <summary>
    /// 永久刪除角色（僅限超級管理員）
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [SuperAdmin]
    public async Task<ActionResult> HardDeleteRole(int id)
    {
        var result = await _roleService.HardDeleteRoleAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的角色" });
        }
        return NoContent();
    }
}
