using Microsoft.AspNetCore.Authorization;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Authorization;

/// <summary>
/// 權限檢查屬性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAttribute : AuthorizeAttribute
{
    public string FunctionCode { get; }
    public PermissionType PermissionType { get; }

    public PermissionAttribute(string functionCode, PermissionType permissionType)
        : base(policy: $"Permission_{functionCode}_{permissionType}")
    {
        FunctionCode = functionCode;
        PermissionType = permissionType;
    }
}
