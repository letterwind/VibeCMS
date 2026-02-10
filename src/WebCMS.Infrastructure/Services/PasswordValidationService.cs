using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WebCMS.Core.Interfaces;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 密碼驗證服務實作
/// </summary>
public class PasswordValidationService : IPasswordValidationService
{
    private const int MinLength = 6;
    private const int MaxLength = 12;
    private const int PasswordExpirationMonths = 3;

    /// <inheritdoc />
    public ValidationResult ValidateAccount(string account)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(account))
        {
            errors.Add("帳號為必填欄位");
            return ValidationResult.Failure(errors);
        }

        if (account.Length < MinLength || account.Length > MaxLength)
        {
            errors.Add($"帳號長度必須為 {MinLength}-{MaxLength} 字元");
        }

        if (!Regex.IsMatch(account, @"[A-Z]"))
        {
            errors.Add("帳號必須包含至少 1 個大寫字母");
        }

        if (!Regex.IsMatch(account, @"[a-z]"))
        {
            errors.Add("帳號必須包含至少 1 個小寫字母");
        }

        if (!Regex.IsMatch(account, @"[0-9]"))
        {
            errors.Add("帳號必須包含至少 1 個數字");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <inheritdoc />
    public ValidationResult ValidatePassword(string password, string account)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("密碼為必填欄位");
            return ValidationResult.Failure(errors);
        }

        if (password.Length < MinLength || password.Length > MaxLength)
        {
            errors.Add($"密碼長度必須為 {MinLength}-{MaxLength} 字元");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            errors.Add("密碼必須包含至少 1 個大寫字母");
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            errors.Add("密碼必須包含至少 1 個小寫字母");
        }

        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            errors.Add("密碼必須包含至少 1 個數字");
        }

        // 檢查密碼是否包含帳號內容
        if (!string.IsNullOrEmpty(account) && 
            password.Contains(account, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("密碼不得包含帳號內容");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <inheritdoc />
    public bool IsPasswordExpired(DateTime lastPasswordChange)
    {
        return lastPasswordChange.AddMonths(PasswordExpirationMonths) < DateTime.UtcNow;
    }

    /// <inheritdoc />
    public bool IsSameAsCurrentPassword(string newPassword, string currentPasswordHash)
    {
        if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(currentPasswordHash))
        {
            return false;
        }

        return VerifyPassword(newPassword, currentPasswordHash);
    }

    /// <summary>
    /// 驗證密碼是否與雜湊值匹配
    /// </summary>
    private static bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);

            if (hashBytes.Length != 48)
            {
                return false;
            }

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
        catch
        {
            return false;
        }
    }
}
