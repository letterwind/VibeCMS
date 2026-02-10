using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Core.DTOs.Auth;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 認證控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICaptchaService _captchaService;

    public AuthController(IAuthService authService, ICaptchaService captchaService)
    {
        _authService = authService;
        _captchaService = captchaService;
    }

    /// <summary>
    /// 產生驗證碼
    /// </summary>
    [HttpGet("captcha")]
    [AllowAnonymous]
    public ActionResult<CaptchaResponse> GenerateCaptcha()
    {
        var captcha = _captchaService.Generate();
        return Ok(captcha);
    }

    /// <summary>
    /// 登入
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _authService.ValidateCredentialsAsync(
            request.Account,
            request.Password,
            request.Captcha,
            request.CaptchaToken);

        // 記錄登入嘗試
        await _authService.RecordLoginAttemptAsync(request.Account, result.Success, ipAddress);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        if (result.User == null)
        {
            return Unauthorized(new { message = "登入失敗" });
        }

        var token = _authService.GenerateJwtToken(result.User);
        var refreshToken = _authService.GenerateRefreshToken();

        var userDto = new UserDto(
            result.User.Id,
            result.User.Account,
            result.User.DisplayName,
            result.IsPasswordExpired,
            result.User.CreatedAt,
            result.User.UpdatedAt);

        var response = new LoginResponse(
            token,
            refreshToken,
            userDto,
            DateTime.UtcNow.AddHours(1));

        return Ok(response);
    }

    /// <summary>
    /// 登出
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        // JWT 是無狀態的，登出主要由前端處理（清除 Token）
        // 這裡可以實作 Token 黑名單機制（如果需要）
        return Ok(new { message = "登出成功" });
    }

    /// <summary>
    /// 變更密碼
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "無效的使用者" });
        }

        var success = await _authService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword);

        if (!success)
        {
            return BadRequest(new { message = "密碼變更失敗，請確認目前密碼是否正確，且新密碼不得與目前密碼相同" });
        }

        return Ok(new { message = "密碼變更成功" });
    }
}
