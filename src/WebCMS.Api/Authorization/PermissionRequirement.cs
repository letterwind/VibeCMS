using Microsoft.AspNetCore.Authorization;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Authorization;

/// <summary>
/// 權限需求
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string FunctionCode { get; }
    public PermissionType PermissionType { get; }

    public PermissionRequirement(string functionCode, PermissionType permissionType)
    {
        FunctionCode = functionCode;
        PermissionType = permissionType;
    }
}
