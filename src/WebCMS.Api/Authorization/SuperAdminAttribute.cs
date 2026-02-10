using Microsoft.AspNetCore.Authorization;

namespace WebCMS.Api.Authorization;

/// <summary>
/// 超級管理員權限檢查屬性 - 僅允許超級管理員（階層等級為 1）存取
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class SuperAdminAttribute : AuthorizeAttribute
{
    public const string PolicyName = "SuperAdmin";

    public SuperAdminAttribute() : base(policy: PolicyName)
    {
    }
}
