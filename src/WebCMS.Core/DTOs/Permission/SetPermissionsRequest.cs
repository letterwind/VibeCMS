using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.Permission;

/// <summary>
/// 設定權限請求
/// </summary>
public record SetPermissionsRequest(
    [Required] List<PermissionSetting> Permissions
);

/// <summary>
/// 單一權限設定
/// </summary>
public record PermissionSetting(
    [Required] int FunctionId,
    bool CanCreate,
    bool CanRead,
    bool CanUpdate,
    bool CanDelete
);
