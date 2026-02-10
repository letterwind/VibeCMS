using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WebCMS.Core.Configuration;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;
using WebCMS.Infrastructure.Services;

namespace WebCMS.Tests.Auth;

/// <summary>
/// 認證模組屬性測試
/// Feature: web-cms-management
/// </summary>
public class AuthPropertyTests
{
    private readonly JwtSettings _jwtSettings = new()
    {
        SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposes123!",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpirationMinutes = 60
    };

    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private IMemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>
    /// Property 1: 登入憑證驗證
    /// 對於任何有效的帳號、密碼及正確的驗證碼組合，系統應該成功驗證並回傳有效的 JWT Token。
    /// **Validates: Requirements 1.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ValidCredentials_ShouldReturnValidJwtToken(PositiveInt seed)
    {
        // Generate valid credentials
        var (account, password) = GenerateValidCredentials(seed.Get);

        // Arrange
        using var context = CreateInMemoryContext();
        var cache = CreateMemoryCache();
        var captchaService = new CaptchaService(cache);
        var authService = new AuthService(
            context,
            captchaService,
            Options.Create(_jwtSettings));

        // Create user with hashed password
        var user = new AdminUser
        {
            Account = account,
            PasswordHash = HashPassword(password),
            DisplayName = "Test User",
            PasswordLastChanged = DateTime.UtcNow
        };
        context.AdminUsers.Add(user);
        context.SaveChanges();

        // Generate valid captcha
        var captchaResponse = captchaService.Generate();
        var captchaText = GetCaptchaTextFromToken(cache, captchaResponse.Token);

        // Act
        var result = authService.ValidateCredentialsAsync(
            account, password, captchaText, captchaResponse.Token).Result;

        // Assert
        if (result.Success && result.User != null)
        {
            var token = authService.GenerateJwtToken(result.User);
            return !string.IsNullOrEmpty(token) && token.Split('.').Length == 3;
        }
        return result.Success;
    }

    /// <summary>
    /// Property 3: 驗證碼驗證
    /// 對於任何錯誤的驗證碼輸入，系統應該拒絕登入請求，無論帳號密碼是否正確。
    /// **Validates: Requirements 1.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool InvalidCaptcha_ShouldRejectLogin(PositiveInt seed, NonEmptyString invalidCaptcha)
    {
        // Generate valid credentials
        var (account, password) = GenerateValidCredentials(seed.Get);
        var wrongCaptcha = invalidCaptcha.Get;

        // Arrange
        using var context = CreateInMemoryContext();
        var cache = CreateMemoryCache();
        var captchaService = new CaptchaService(cache);
        var authService = new AuthService(
            context,
            captchaService,
            Options.Create(_jwtSettings));

        // Create user with valid credentials
        var user = new AdminUser
        {
            Account = account,
            PasswordHash = HashPassword(password),
            DisplayName = "Test User",
            PasswordLastChanged = DateTime.UtcNow
        };
        context.AdminUsers.Add(user);
        context.SaveChanges();

        // Generate captcha but use wrong value
        var captchaResponse = captchaService.Generate();
        var correctCaptcha = GetCaptchaTextFromToken(cache, captchaResponse.Token);

        // Skip if the random captcha happens to match
        if (wrongCaptcha.Equals(correctCaptcha, StringComparison.OrdinalIgnoreCase))
            return true;

        // Act
        var result = authService.ValidateCredentialsAsync(
            account, password, wrongCaptcha, captchaResponse.Token).Result;

        // Assert - should fail due to invalid captcha
        return !result.Success && result.ErrorMessage == "驗證碼錯誤";
    }

    /// <summary>
    /// Property 4: 安全錯誤訊息
    /// 對於任何帳號錯誤或密碼錯誤的登入嘗試，系統應該回傳相同的錯誤訊息，不透露具體是哪個欄位錯誤。
    /// **Validates: Requirements 1.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool InvalidCredentials_ShouldReturnGenericErrorMessage(PositiveInt seed1, PositiveInt seed2)
    {
        // Generate two different sets of credentials
        var (account, correctPassword) = GenerateValidCredentials(seed1.Get);
        var (_, wrongPassword) = GenerateValidCredentials(seed2.Get + 1000);

        // Skip if passwords happen to be the same
        if (correctPassword == wrongPassword) return true;

        // Arrange
        using var context = CreateInMemoryContext();
        var cache = CreateMemoryCache();
        var captchaService = new CaptchaService(cache);
        var authService = new AuthService(
            context,
            captchaService,
            Options.Create(_jwtSettings));

        // Create user
        var user = new AdminUser
        {
            Account = account,
            PasswordHash = HashPassword(correctPassword),
            DisplayName = "Test User",
            PasswordLastChanged = DateTime.UtcNow
        };
        context.AdminUsers.Add(user);
        context.SaveChanges();

        // Generate valid captcha for wrong password test
        var captchaResponse1 = captchaService.Generate();
        var captchaText1 = GetCaptchaTextFromToken(cache, captchaResponse1.Token);

        // Test with wrong password
        var resultWrongPassword = authService.ValidateCredentialsAsync(
            account, wrongPassword, captchaText1, captchaResponse1.Token).Result;

        // Generate another captcha for wrong account test
        var captchaResponse2 = captchaService.Generate();
        var captchaText2 = GetCaptchaTextFromToken(cache, captchaResponse2.Token);

        // Test with wrong account
        var resultWrongAccount = authService.ValidateCredentialsAsync(
            "NonExistent1", correctPassword, captchaText2, captchaResponse2.Token).Result;

        // Assert - both should return the same generic error message
        const string expectedError = "帳號或密碼錯誤";
        return !resultWrongPassword.Success && 
               !resultWrongAccount.Success &&
               resultWrongPassword.ErrorMessage == expectedError &&
               resultWrongAccount.ErrorMessage == expectedError;
    }

    #region Helpers

    private static (string account, string password) GenerateValidCredentials(int seed)
    {
        var random = new Random(seed);
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string allChars = upperChars + lowerChars + digitChars;

        // Generate account (6-12 chars with at least 1 upper, 1 lower, 1 digit)
        var accountLength = random.Next(6, 13);
        var accountChars = new char[accountLength];
        accountChars[0] = upperChars[random.Next(upperChars.Length)];
        accountChars[1] = lowerChars[random.Next(lowerChars.Length)];
        accountChars[2] = digitChars[random.Next(digitChars.Length)];
        for (int i = 3; i < accountLength; i++)
        {
            accountChars[i] = allChars[random.Next(allChars.Length)];
        }
        Shuffle(accountChars, random);

        // Generate password (6-12 chars with at least 1 upper, 1 lower, 1 digit)
        var passwordLength = random.Next(6, 13);
        var passwordChars = new char[passwordLength];
        passwordChars[0] = upperChars[random.Next(upperChars.Length)];
        passwordChars[1] = lowerChars[random.Next(lowerChars.Length)];
        passwordChars[2] = digitChars[random.Next(digitChars.Length)];
        for (int i = 3; i < passwordLength; i++)
        {
            passwordChars[i] = allChars[random.Next(allChars.Length)];
        }
        Shuffle(passwordChars, random);

        return (new string(accountChars), new string(passwordChars));
    }

    private static void Shuffle(char[] array, Random random)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private static string HashPassword(string password)
    {
        var salt = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);

        var hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            System.Text.Encoding.UTF8.GetBytes(password),
            salt,
            100000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    private static string GetCaptchaTextFromToken(IMemoryCache cache, string token)
    {
        var cacheKey = $"captcha:{token}";
        if (cache.TryGetValue(cacheKey, out string? captchaText))
        {
            return captchaText ?? string.Empty;
        }
        return string.Empty;
    }

    #endregion
}
