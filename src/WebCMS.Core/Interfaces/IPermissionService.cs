using WebCMS.Core.DTOs.Permission;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 權限類型列舉
/// </summary>
public enum PermissionType
{
    Create,
    Read,
    Update,
    Delete
}

/// <summary>
/// 權限服務介面
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 取得角色的所有權限
    /// </summary>
    Task<List<PermissionDto>> GetPermissionsByRoleAsync(int roleId);

    /// <summary>
    /// 設定角色的權限
    /// </summary>
    Task SetPermissionsAsync(int roleId, List<PermissionSetting> permissions);

    /// <summary>
    /// 檢查使用者是否有特定功能的權限
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string functionCode, PermissionType type);

    /// <summary>
    /// 取得角色的功能權限樹狀結構
    /// </summary>
    Task<List<FunctionPermissionDto>> GetFunctionPermissionsAsync(int roleId);

    /// <summary>
    /// 取得使用者的所有權限（合併所有角色的權限）
    /// </summary>
    Task<List<PermissionDto>> GetUserPermissionsAsync(int userId);

    /// <summary>
    /// 檢查角色是否有特定功能的權限
    /// </summary>
    Task<bool> RoleHasPermissionAsync(int roleId, string functionCode, PermissionType type);

    /// <summary>
    /// 檢查使用者是否為超級管理員（階層等級為 1）
    /// </summary>
    Task<bool> IsSuperAdminAsync(int userId);
}
