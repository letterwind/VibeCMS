using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Permission;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 權限服務實作
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionDto>> GetPermissionsByRoleAsync(int roleId)
    {
        var permissions = await _context.RolePermissions
            .Include(rp => rp.Function)
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => new PermissionDto(
                rp.FunctionId,
                rp.Function.Name,
                rp.Function.Code,
                rp.CanCreate,
                rp.CanRead,
                rp.CanUpdate,
                rp.CanDelete))
            .ToListAsync();

        return permissions;
    }

    public async Task SetPermissionsAsync(int roleId, List<PermissionSetting> permissions)
    {
        // 驗證角色是否存在
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
        if (!roleExists)
        {
            throw new ArgumentException($"角色 ID {roleId} 不存在");
        }

        // 移除該角色的所有現有權限
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        _context.RolePermissions.RemoveRange(existingPermissions);

        // 新增新的權限設定
        foreach (var permission in permissions)
        {
            // 驗證功能是否存在
            var functionExists = await _context.Functions.AnyAsync(f => f.Id == permission.FunctionId);
            if (!functionExists)
            {
                throw new ArgumentException($"功能 ID {permission.FunctionId} 不存在");
            }

            // 只有當至少有一個權限為 true 時才新增記錄
            if (permission.CanCreate || permission.CanRead || permission.CanUpdate || permission.CanDelete)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    FunctionId = permission.FunctionId,
                    CanCreate = permission.CanCreate,
                    CanRead = permission.CanRead,
                    CanUpdate = permission.CanUpdate,
                    CanDelete = permission.CanDelete
                };
                _context.RolePermissions.Add(rolePermission);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasPermissionAsync(int userId, string functionCode, PermissionType type)
    {
        // 取得使用者的所有角色
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!roleIds.Any())
        {
            return false;
        }

        // 取得功能 ID
        var function = await _context.Functions
            .FirstOrDefaultAsync(f => f.Code == functionCode);

        if (function == null)
        {
            return false;
        }

        // 取得相關權限記錄
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && rp.FunctionId == function.Id)
            .ToListAsync();

        // 在記憶體中檢查權限
        return type switch
        {
            PermissionType.Create => permissions.Any(rp => rp.CanCreate),
            PermissionType.Read => permissions.Any(rp => rp.CanRead),
            PermissionType.Update => permissions.Any(rp => rp.CanUpdate),
            PermissionType.Delete => permissions.Any(rp => rp.CanDelete),
            _ => false
        };
    }

    public async Task<List<FunctionPermissionDto>> GetFunctionPermissionsAsync(int roleId)
    {
        // 取得所有功能
        var functions = await _context.Functions
            .OrderBy(f => f.SortOrder)
            .ToListAsync();

        // 取得角色的權限
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToDictionaryAsync(rp => rp.FunctionId);

        // 建立樹狀結構
        var rootFunctions = functions.Where(f => f.ParentId == null).ToList();
        return BuildFunctionPermissionTree(rootFunctions, functions, permissions);
    }

    private List<FunctionPermissionDto> BuildFunctionPermissionTree(
        List<Function> currentLevel,
        List<Function> allFunctions,
        Dictionary<int, RolePermission> permissions)
    {
        var result = new List<FunctionPermissionDto>();

        foreach (var function in currentLevel)
        {
            var permission = permissions.GetValueOrDefault(function.Id);
            var children = allFunctions.Where(f => f.ParentId == function.Id).ToList();

            var dto = new FunctionPermissionDto(
                function.Id,
                function.Name,
                function.Code,
                function.Icon,
                function.ParentId,
                function.SortOrder,
                permission?.CanCreate ?? false,
                permission?.CanRead ?? false,
                permission?.CanUpdate ?? false,
                permission?.CanDelete ?? false,
                children.Any() ? BuildFunctionPermissionTree(children, allFunctions, permissions) : null);

            result.Add(dto);
        }

        return result;
    }

    public async Task<List<PermissionDto>> GetUserPermissionsAsync(int userId)
    {
        // 取得使用者的所有角色
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!roleIds.Any())
        {
            return new List<PermissionDto>();
        }

        // 取得所有角色的權限並合併（取最大權限）
        var permissions = await _context.RolePermissions
            .Include(rp => rp.Function)
            .Where(rp => roleIds.Contains(rp.RoleId))
            .GroupBy(rp => new { rp.FunctionId, rp.Function.Name, rp.Function.Code })
            .Select(g => new PermissionDto(
                g.Key.FunctionId,
                g.Key.Name,
                g.Key.Code,
                g.Any(rp => rp.CanCreate),
                g.Any(rp => rp.CanRead),
                g.Any(rp => rp.CanUpdate),
                g.Any(rp => rp.CanDelete)))
            .ToListAsync();

        return permissions;
    }

    public async Task<bool> RoleHasPermissionAsync(int roleId, string functionCode, PermissionType type)
    {
        // 取得功能 ID
        var function = await _context.Functions
            .FirstOrDefaultAsync(f => f.Code == functionCode);

        if (function == null)
        {
            return false;
        }

        // 檢查角色是否有該權限
        var permission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.FunctionId == function.Id);

        if (permission == null)
        {
            return false;
        }

        return type switch
        {
            PermissionType.Create => permission.CanCreate,
            PermissionType.Read => permission.CanRead,
            PermissionType.Update => permission.CanUpdate,
            PermissionType.Delete => permission.CanDelete,
            _ => false
        };
    }

    public async Task<bool> IsSuperAdminAsync(int userId)
    {
        // 取得使用者的所有角色
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId && !ur.Role.IsDeleted)
            .ToListAsync();

        // 檢查是否有任何角色的階層等級為 1（超級管理員）
        return userRoles.Any(ur => ur.Role.HierarchyLevel == 1);
    }
}
