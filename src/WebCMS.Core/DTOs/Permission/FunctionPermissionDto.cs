namespace WebCMS.Core.DTOs.Permission;

/// <summary>
/// 功能權限 DTO - 用於顯示功能及其權限設定
/// </summary>
public record FunctionPermissionDto(
    int FunctionId,
    string FunctionName,
    string FunctionCode,
    string? Icon,
    int? ParentId,
    int SortOrder,
    bool CanCreate,
    bool CanRead,
    bool CanUpdate,
    bool CanDelete,
    List<FunctionPermissionDto>? Children = null
);
