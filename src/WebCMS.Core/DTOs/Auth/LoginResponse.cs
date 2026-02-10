namespace WebCMS.Core.DTOs.Auth;

/// <summary>
/// 登入回應 DTO
/// </summary>
public record LoginResponse(
    string Token,
    string RefreshToken,
    UserDto User,
    DateTime ExpiresAt
);

/// <summary>
/// 使用者資訊 DTO
/// </summary>
public record UserDto(
    int Id,
    string Account,
    string DisplayName,
    bool IsPasswordExpired,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
