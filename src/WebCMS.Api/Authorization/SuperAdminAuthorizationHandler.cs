using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Authorization;

/// <summary>
/// 超級管理員授權處理器
/// </summary>
public class SuperAdminAuthorizationHandler : AuthorizationHandler<SuperAdminRequirement>
{
    private readonly IPermissionService _permissionService;

    public SuperAdminAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperAdminRequirement requirement)
    {
        // 取得使用者 ID
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        // 檢查是否為超級管理員
        var isSuperAdmin = await _permissionService.IsSuperAdminAsync(userId);

        if (isSuperAdmin)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
