using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using System.Security.Cryptography;
using System.Text;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Services;

namespace WebCMS.Tests.Auth;

/// <summary>
/// 密碼驗證模組屬性測試
/// Feature: web-cms-management
/// </summary>
public class PasswordValidationPropertyTests
{
    private readonly IPasswordValidationService _service = new PasswordValidationService();

    #region Property 5: 憑證格式驗證

    /// <summary>
    /// Property 5: 憑證格式驗證 - 有效帳號應被接受
    /// 對於任何帳號或密碼字串，若長度在 6-12 字元範圍內，且包含大寫字母、小寫字母、數字，系統應該接受該憑證。
    /// **Validates: Requirements 4.2, 4.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ValidAccount_ShouldBeAccepted(PositiveInt seed)
    {
        var account = GenerateValidCredential(seed.Get);
        var result = _service.ValidateAccount(account);
        return result.IsValid;
    }

    /// <summary>
    /// Property 5: 憑證格式驗證 - 有效密碼應被接受
    /// **Validates: Requirements 4.2, 4.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ValidPassword_ShouldBeAccepted(PositiveInt seed1, PositiveInt seed2)
    {
        var password = GenerateValidCredential(seed1.Get);
        var account = GenerateValidCredential(seed2.Get + 10000); // Different seed to avoid overlap
        
        // Ensure password doesn't contain account
        if (password.Contains(account, StringComparison.OrdinalIgnoreCase))
            return true; // Skip this case
            
        var result = _service.ValidatePassword(password, account);
        return result.IsValid;
    }

    /// <summary>
    /// Property 5: 憑證格式驗證 - 長度不足的帳號應被拒絕
    /// **Validates: Requirements 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool TooShortAccount_ShouldBeRejected(PositiveInt seed)
    {
        var shortAccount = GenerateCredentialWithLength(seed.Get, 1, 5);
        var result = _service.ValidateAccount(shortAccount);
        return !result.IsValid && result.Errors.Any(e => e.Contains("6-12"));
    }

    /// <summary>
    /// Property 5: 憑證格式驗證 - 長度過長的帳號應被拒絕
    /// **Validates: Requirements 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool TooLongAccount_ShouldBeRejected(PositiveInt seed)
    {
        var longAccount = GenerateCredentialWithLength(seed.Get, 13, 20);
        var result = _service.ValidateAccount(longAccount);
        return !result.IsValid && result.Errors.Any(e => e.Contains("6-12"));
    }

    /// <summary>
    /// Property 5: 憑證格式驗證 - 缺少大寫字母的帳號應被拒絕
    /// **Validates: Requirements 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AccountWithoutUppercase_ShouldBeRejected(PositiveInt seed)
    {
        var account = GenerateCredentialMissingUppercase(seed.Get);
        var result = _service.ValidateAccount(account);
        return !result.IsValid && result.Errors.Any(e => e.Contains("大寫字母"));
    }

    /// <summary>
    /// Property 5: 憑證格式驗證 - 缺少小寫字母的帳號應被拒絕
    /// **Validates: Requirements 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AccountWithoutLowercase_ShouldBeRejected(PositiveInt seed)
    {
        var account = GenerateCredentialMissingLowercase(seed.Get);
        var result = _service.ValidateAccount(account);
        return !result.IsValid && result.Errors.Any(e => e.Contains("小寫字母"));
    }

    /// <summary>
    /// Property 5: 憑證格式驗證 - 缺少數字的帳號應被拒絕
    /// **Validates: Requirements 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AccountWithoutDigit_ShouldBeRejected(PositiveInt seed)
    {
        var account = GenerateCredentialMissingDigit(seed.Get);
        var result = _service.ValidateAccount(account);
        return !result.IsValid && result.Errors.Any(e => e.Contains("數字"));
    }

    #endregion

    #region Property 6: 密碼不含帳號驗證

    /// <summary>
    /// Property 6: 密碼不含帳號驗證
    /// 對於任何包含帳號內容的密碼，系統應該拒絕該密碼設定。
    /// **Validates: Requirements 4.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PasswordContainingAccount_ShouldBeRejected(PositiveInt seed)
    {
        var account = GenerateValidCredential(seed.Get);
        // Create password that contains the account
        var password = account + "X1"; // Append to ensure it still meets length requirements
        
        // Ensure password is within valid length
        if (password.Length > 12)
            password = password.Substring(0, 12);
            
        var result = _service.ValidatePassword(password, account);
        return !result.IsValid && result.Errors.Any(e => e.Contains("帳號內容"));
    }

    /// <summary>
    /// Property 6: 密碼不含帳號驗證 - 大小寫不敏感
    /// **Validates: Requirements 4.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PasswordContainingAccountCaseInsensitive_ShouldBeRejected(PositiveInt seed)
    {
        // Generate a short account (6 chars) to leave room for additional chars
        var random = new Random(seed.Get);
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        
        // Create a 6-char account with required characters
        var accountChars = new char[6];
        accountChars[0] = upperChars[random.Next(upperChars.Length)];
        accountChars[1] = lowerChars[random.Next(lowerChars.Length)];
        accountChars[2] = digitChars[random.Next(digitChars.Length)];
        accountChars[3] = lowerChars[random.Next(lowerChars.Length)];
        accountChars[4] = upperChars[random.Next(upperChars.Length)];
        accountChars[5] = digitChars[random.Next(digitChars.Length)];
        var account = new string(accountChars);
        
        // Create password that contains the account in different case
        // Password format: "X" + lowercase(account) + "1" = 8 chars total
        // This ensures: uppercase (X), lowercase (from account), digit (1), and contains account
        var password = "X" + account.ToLower() + "1";
        
        // Verify password length is valid (should be 8 chars)
        if (password.Length < 6 || password.Length > 12)
            return true; // Skip invalid case
        
        var result = _service.ValidatePassword(password, account);
        return !result.IsValid && result.Errors.Any(e => e.Contains("帳號內容"));
    }

    #endregion

    #region Property 7: 密碼變更不重複驗證

    /// <summary>
    /// Property 7: 密碼變更不重複驗證
    /// 對於任何與目前密碼相同的新密碼，系統應該拒絕該密碼變更請求。
    /// **Validates: Requirements 4.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SamePassword_ShouldBeDetected(PositiveInt seed)
    {
        var password = GenerateValidCredential(seed.Get);
        var hashedPassword = HashPassword(password);
        
        var isSame = _service.IsSameAsCurrentPassword(password, hashedPassword);
        return isSame;
    }

    /// <summary>
    /// Property 7: 密碼變更不重複驗證 - 不同密碼應被允許
    /// **Validates: Requirements 4.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool DifferentPassword_ShouldNotBeDetectedAsSame(PositiveInt seed1, PositiveInt seed2)
    {
        var currentPassword = GenerateValidCredential(seed1.Get);
        var newPassword = GenerateValidCredential(seed2.Get + 10000);
        
        // Skip if passwords happen to be the same
        if (currentPassword == newPassword)
            return true;
            
        var hashedCurrentPassword = HashPassword(currentPassword);
        
        var isSame = _service.IsSameAsCurrentPassword(newPassword, hashedCurrentPassword);
        return !isSame;
    }

    #endregion

    #region Helpers

    private static string GenerateValidCredential(int seed)
    {
        var random = new Random(seed);
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string allChars = upperChars + lowerChars + digitChars;

        var length = random.Next(6, 13);
        var chars = new char[length];
        chars[0] = upperChars[random.Next(upperChars.Length)];
        chars[1] = lowerChars[random.Next(lowerChars.Length)];
        chars[2] = digitChars[random.Next(digitChars.Length)];
        for (int i = 3; i < length; i++)
        {
            chars[i] = allChars[random.Next(allChars.Length)];
        }
        Shuffle(chars, random);

        return new string(chars);
    }

    private static string GenerateCredentialWithLength(int seed, int minLength, int maxLength)
    {
        var random = new Random(seed);
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string allChars = upperChars + lowerChars + digitChars;

        var length = random.Next(minLength, maxLength + 1);
        var chars = new char[length];
        
        // Ensure at least one of each type if length allows
        int index = 0;
        if (length > 0) chars[index++] = upperChars[random.Next(upperChars.Length)];
        if (length > 1) chars[index++] = lowerChars[random.Next(lowerChars.Length)];
        if (length > 2) chars[index++] = digitChars[random.Next(digitChars.Length)];
        
        for (int i = index; i < length; i++)
        {
            chars[i] = allChars[random.Next(allChars.Length)];
        }
        Shuffle(chars, random);

        return new string(chars);
    }

    private static string GenerateCredentialMissingUppercase(int seed)
    {
        var random = new Random(seed);
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string allChars = lowerChars + digitChars;

        var length = random.Next(6, 13);
        var chars = new char[length];
        chars[0] = lowerChars[random.Next(lowerChars.Length)];
        chars[1] = digitChars[random.Next(digitChars.Length)];
        for (int i = 2; i < length; i++)
        {
            chars[i] = allChars[random.Next(allChars.Length)];
        }
        Shuffle(chars, random);

        return new string(chars);
    }

    private static string GenerateCredentialMissingLowercase(int seed)
    {
        var random = new Random(seed);
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digitChars = "0123456789";
        const string allChars = upperChars + digitChars;

        var length = random.Next(6, 13);
        var chars = new char[length];
        chars[0] = upperChars[random.Next(upperChars.Length)];
        chars[1] = digitChars[random.Next(digitChars.Length)];
        for (int i = 2; i < length; i++)
        {
            chars[i] = allChars[random.Next(allChars.Length)];
        }
        Shuffle(chars, random);

        return new string(chars);
    }

    private static string GenerateCredentialMissingDigit(int seed)
    {
        var random = new Random(seed);
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string allChars = upperChars + lowerChars;

        var length = random.Next(6, 13);
        var chars = new char[length];
        chars[0] = upperChars[random.Next(upperChars.Length)];
        chars[1] = lowerChars[random.Next(lowerChars.Length)];
        for (int i = 2; i < length; i++)
        {
            chars[i] = allChars[random.Next(allChars.Length)];
        }
        Shuffle(chars, random);

        return new string(chars);
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

    #endregion
}
