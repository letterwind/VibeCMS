namespace WebCMS.Core.Configuration;

/// <summary>
/// JWT 設定
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// 密鑰
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 發行者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 接收者
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token 過期時間（分鐘）
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh Token 過期時間（天）
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
