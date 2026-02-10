using Microsoft.AspNetCore.Authorization;

namespace WebCMS.Api.Authorization;

/// <summary>
/// 超級管理員需求 - 僅允許超級管理員（階層等級為 1）存取
/// </summary>
public class SuperAdminRequirement : IAuthorizationRequirement
{
}
