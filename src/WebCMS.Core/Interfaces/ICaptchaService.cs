using WebCMS.Core.DTOs.Auth;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 驗證碼服務介面
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// 產生驗證碼
    /// </summary>
    /// <returns>驗證碼回應（包含 Base64 圖片和 Token）</returns>
    CaptchaResponse Generate();

    /// <summary>
    /// 驗證驗證碼
    /// </summary>
    /// <param name="captcha">使用者輸入的驗證碼</param>
    /// <param name="token">驗證碼 Token</param>
    /// <returns>驗證是否成功</returns>
    bool Validate(string captcha, string token);
}
