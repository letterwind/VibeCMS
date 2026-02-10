namespace WebCMS.Core.DTOs.Permission;

/// <summary>
/// 權限 DTO
/// </summary>
public record PermissionDto(
    int FunctionId,
    string FunctionName,
    string FunctionCode,
    bool CanCreate,
    bool CanRead,
    bool CanUpdate,
    bool CanDelete
);
