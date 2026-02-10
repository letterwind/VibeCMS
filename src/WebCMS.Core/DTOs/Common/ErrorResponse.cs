namespace WebCMS.Core.DTOs.Common;

/// <summary>
/// 錯誤回應格式
/// 用於統一 API 錯誤回應結構
/// </summary>
public record ErrorResponse(
    string Code,
    string Message,
    Dictionary<string, string[]>? Errors = null,
    string? TraceId = null
);

/// <summary>
/// 錯誤代碼定義
/// </summary>
public static class ErrorCodes
{
    // 認證錯誤
    public const string InvalidCredentials = "AUTH_001";
    public const string AccountLocked = "AUTH_002";
    public const string InvalidCaptcha = "AUTH_003";
    public const string TokenExpired = "AUTH_004";
    public const string PasswordExpired = "AUTH_005";

    // 驗證錯誤
    public const string ValidationFailed = "VAL_001";
    public const string RequiredFieldMissing = "VAL_002";
    public const string InvalidFormat = "VAL_003";
    public const string MaxLengthExceeded = "VAL_004";

    // 權限錯誤
    public const string AccessDenied = "PERM_001";
    public const string InsufficientPermission = "PERM_002";

    // 資源錯誤
    public const string ResourceNotFound = "RES_001";
    public const string ResourceAlreadyExists = "RES_002";
    public const string ResourceInUse = "RES_003";

    // 業務邏輯錯誤
    public const string MaxCategoryDepthExceeded = "BIZ_001";
    public const string CannotDeleteWithChildren = "BIZ_002";
    public const string SingleRecordOnly = "BIZ_003";
    public const string PasswordSameAsCurrent = "BIZ_004";
    public const string PasswordContainsAccount = "BIZ_005";
}
