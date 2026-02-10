namespace WebCMS.Core.DTOs.Auth;

/// <summary>
/// 驗證碼回應 DTO
/// </summary>
public record CaptchaResponse(
    /// <summary>
    /// Base64 編碼的驗證碼圖片
    /// </summary>
    string ImageBase64,

    /// <summary>
    /// 驗證碼 Token（用於驗證時比對）
    /// </summary>
    string Token
);
