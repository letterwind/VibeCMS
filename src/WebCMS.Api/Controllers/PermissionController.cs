using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.DTOs.Permission;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 權限管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// 取得角色的所有權限
    /// </summary>
    [HttpGet("role/{roleId}")]
    public async Task<ActionResult<List<PermissionDto>>> GetPermissions(int roleId)
    {
        var permissions = await _permissionService.GetPermissionsByRoleAsync(roleId);
        return Ok(permissions);
    }

    /// <summary>
    /// 設定角色的權限
    /// </summary>
    [HttpPost("role/{roleId}")]
    public async Task<ActionResult> SetPermissions(int roleId, [FromBody] SetPermissionsRequest request)
    {
        try
        {
            await _permissionService.SetPermissionsAsync(roleId, request.Permissions);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 取得角色的功能權限樹狀結構
    /// </summary>
    [HttpGet("role/{roleId}/functions")]
    public async Task<ActionResult<List<FunctionPermissionDto>>> GetFunctionPermissions(int roleId)
    {
        var permissions = await _permissionService.GetFunctionPermissionsAsync(roleId);
        return Ok(permissions);
    }

    /// <summary>
    /// 取得使用者的所有權限（合併所有角色的權限）
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<PermissionDto>>> GetUserPermissions(int userId)
    {
        var permissions = await _permissionService.GetUserPermissionsAsync(userId);
        return Ok(permissions);
    }

    /// <summary>
    /// 檢查使用者是否有特定功能的權限
    /// </summary>
    [HttpGet("check")]
    public async Task<ActionResult<bool>> CheckPermission(
        [FromQuery] int userId,
        [FromQuery] string functionCode,
        [FromQuery] PermissionType type)
    {
        var hasPermission = await _permissionService.HasPermissionAsync(userId, functionCode, type);
        return Ok(hasPermission);
    }
}
