using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Authorization;

/// <summary>
/// 權限授權處理器
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // 取得使用者 ID
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        // 檢查權限
        var hasPermission = await _permissionService.HasPermissionAsync(
            userId,
            requirement.FunctionCode,
            requirement.PermissionType);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
