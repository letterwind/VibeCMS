using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebCMS.Core.Configuration;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 認證服務實作
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ICaptchaService _captchaService;
    private readonly JwtSettings _jwtSettings;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;
    private const int PasswordExpirationMonths = 3;

    public AuthService(
        ApplicationDbContext context,
        ICaptchaService captchaService,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _captchaService = captchaService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResult> ValidateCredentialsAsync(
        string account, string password, string captcha, string captchaToken)
    {
        // 驗證驗證碼
        if (!_captchaService.Validate(captcha, captchaToken))
        {
            return new LoginResult(false, "驗證碼錯誤");
        }

        // 檢查帳號是否被鎖定
        if (await IsAccountLockedAsync(account))
        {
            return new LoginResult(false, "帳號已被鎖定，請稍後再試");
        }

        // 查詢使用者（使用 IgnoreQueryFilters 以便檢查軟刪除的帳號）
        var user = await _context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Account == account);

        // 統一錯誤訊息，不透露具體錯誤原因
        const string genericErrorMessage = "帳號或密碼錯誤";

        if (user == null || user.IsDeleted)
        {
            await IncrementFailedAttemptsAsync(account);
            return new LoginResult(false, genericErrorMessage);
        }

        // 驗證密碼
        if (!VerifyPassword(password, user.PasswordHash))
        {
            await IncrementFailedAttemptsAsync(account);

            // 檢查是否需要鎖定帳號
            var failedAttempts = await GetFailedAttemptsAsync(account);
            if (failedAttempts >= MaxFailedAttempts)
            {
                await LockAccountAsync(account);
            }

            return new LoginResult(false, genericErrorMessage);
        }

        // 登入成功，重設失敗次數
        await ResetFailedAttemptsAsync(account);

        // 檢查密碼是否過期
        var isPasswordExpired = IsPasswordExpired(user.PasswordLastChanged);

        return new LoginResult(true, null, user, isPasswordExpired);
    }

    public async Task<bool> IsAccountLockedAsync(string account)
    {
        var user = await _context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Account == account);

        if (user == null || user.LockoutEnd == null)
            return false;

        // 檢查鎖定是否已過期（自動解鎖）
        if (user.LockoutEnd <= DateTime.UtcNow)
        {
            await UnlockAccountAsync(account);
            return false;
        }

        return true;
    }

    public async Task LockAccountAsync(string account)
    {
        var user = await _context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Account == account);

        if (user != null)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UnlockAccountAsync(string account)
    {
        var user = await _context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Account == account);

        if (user != null)
        {
            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetFailedAttemptsAsync(string account)
    {
        var user = await _context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Account == account);

        return user?.FailedLoginAttempts ?? 0;
    }

    public async Task IncrementFailedAttemptsAsync(string account)
    {
        var user = await _context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Account == account);

        if (user != null)
        {
            user.FailedLoginAttempts++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ResetFailedAttemptsAsync(string account)
    {
        var user = await _context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Account == account);

        if (user != null)
        {
            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();
        }
    }

    public string GenerateJwtToken(AdminUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Account),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public bool IsPasswordExpired(DateTime passwordLastChanged)
    {
        return passwordLastChanged.AddMonths(PasswordExpirationMonths) < DateTime.UtcNow;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.AdminUsers.FindAsync(userId);
        if (user == null)
            return false;

        // 驗證目前密碼
        if (!VerifyPassword(currentPassword, user.PasswordHash))
            return false;

        // 檢查新密碼是否與目前密碼相同
        if (VerifyPassword(newPassword, user.PasswordHash))
            return false;

        // 更新密碼
        user.PasswordHash = HashPassword(newPassword);
        user.PasswordLastChanged = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task RecordLoginAttemptAsync(string account, bool isSuccess, string? ipAddress)
    {
        var attempt = new LoginAttempt
        {
            Account = account,
            IsSuccess = isSuccess,
            IpAddress = ipAddress,
            AttemptedAt = DateTime.UtcNow
        };

        _context.LoginAttempts.Add(attempt);
        await _context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var hashBytes = Convert.FromBase64String(storedHash);

        var salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        for (int i = 0; i < 32; i++)
        {
            if (hashBytes[i + 16] != hash[i])
                return false;
        }

        return true;
    }
}
