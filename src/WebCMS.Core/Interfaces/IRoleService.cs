using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.Role;
using WebCMS.Core.Entities;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 角色服務介面
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// 取得角色列表（分頁）
    /// </summary>
    Task<PagedResult<RoleDto>> GetRolesAsync(QueryParameters query);

    /// <summary>
    /// 取得單一角色
    /// </summary>
    Task<RoleDto?> GetRoleByIdAsync(int id);

    /// <summary>
    /// 建立角色
    /// </summary>
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request);

    /// <summary>
    /// 軟刪除角色
    /// </summary>
    Task<bool> SoftDeleteRoleAsync(int id);

    /// <summary>
    /// 永久刪除角色（僅限超級管理員）
    /// </summary>
    Task<bool> HardDeleteRoleAsync(int id);

    /// <summary>
    /// 檢查角色名稱是否已存在
    /// </summary>
    Task<bool> IsNameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// 取得所有角色（不分頁，用於下拉選單）
    /// </summary>
    Task<List<RoleDto>> GetAllRolesAsync();

    /// <summary>
    /// 根據階層等級取得角色
    /// </summary>
    Task<List<RoleDto>> GetRolesByHierarchyLevelAsync(int maxLevel);
}
