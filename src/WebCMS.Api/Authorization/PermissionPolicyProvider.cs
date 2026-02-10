using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Authorization;

/// <summary>
/// 權限策略提供者 - 動態建立權限策略
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PolicyPrefix = "Permission_";
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        // 解析策略名稱: Permission_{FunctionCode}_{PermissionType}
        var parts = policyName.Substring(PolicyPrefix.Length).Split('_');
        if (parts.Length < 2)
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        var functionCode = string.Join("_", parts.Take(parts.Length - 1));
        var permissionTypeStr = parts.Last();

        if (!Enum.TryParse<PermissionType>(permissionTypeStr, out var permissionType))
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(functionCode, permissionType))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
