using FluentAssertions;
using FluentValidation;
using FsCheck;
using FsCheck.Xunit;
using WebCMS.Core.DTOs.Article;
using WebCMS.Core.DTOs.Auth;
using WebCMS.Core.DTOs.Category;
using WebCMS.Core.DTOs.Function;
using WebCMS.Core.DTOs.Role;
using WebCMS.Core.DTOs.User;
using WebCMS.Core.Validators;

namespace WebCMS.Tests.Validation;

/// <summary>
/// Property 2: 必填欄位驗證
/// 對於任何包含空白必填欄位的表單提交，系統應該拒絕該請求並回傳對應欄位的必填錯誤訊息。
/// Feature: web-cms-management
/// </summary>
public class RequiredFieldValidationPropertyTests
{
    #region Property 2.1: 登入請求必填欄位驗證 (需求 1.2)

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白帳號應被拒絕
    /// **Validates: Requirements 1.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool LoginRequest_EmptyAccount_ShouldBeRejected(NonEmptyString password, NonEmptyString captcha, NonEmptyString token)
    {
        var validator = new LoginRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new LoginRequest(emptyValue!, password.Get, captcha.Get, token.Get);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Account" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白密碼應被拒絕
    /// **Validates: Requirements 1.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool LoginRequest_EmptyPassword_ShouldBeRejected(NonEmptyString account, NonEmptyString captcha, NonEmptyString token)
    {
        var validator = new LoginRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };

        foreach (var emptyValue in emptyValues)
        {
            var request = new LoginRequest(account.Get, emptyValue!, captcha.Get, token.Get);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白驗證碼應被拒絕
    /// **Validates: Requirements 1.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool LoginRequest_EmptyCaptcha_ShouldBeRejected(NonEmptyString account, NonEmptyString password, NonEmptyString token)
    {
        var validator = new LoginRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new LoginRequest(account.Get, password.Get, emptyValue!, token.Get);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Captcha" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 所有登入必填欄位都為空時應回傳所有錯誤
    /// **Validates: Requirements 1.2**
    /// </summary>
    [Fact]
    public void LoginRequest_AllEmptyFields_ShouldReturnAllErrors()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest("", "", "", "");
        var result = validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Account" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "Captcha" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "CaptchaToken" && e.ErrorMessage.Contains("必填"));
    }

    #endregion

    #region Property 2.2: 角色請求必填欄位驗證 (需求 2.2)

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白角色名稱應被拒絕
    /// **Validates: Requirements 2.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateRoleRequest_EmptyName_ShouldBeRejected(PositiveInt hierarchyLevel)
    {
        var validator = new CreateRoleRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateRoleRequest(emptyValue!, "Description", hierarchyLevel.Get);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 有效角色名稱應被接受
    /// **Validates: Requirements 2.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateRoleRequest_ValidName_ShouldBeAccepted(NonEmptyString name, PositiveInt hierarchyLevel)
    {
        // Skip whitespace-only strings as they are considered empty
        var trimmedName = name.Get.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            return true; // Skip this case
            
        var validator = new CreateRoleRequestValidator();
        var validName = trimmedName.Length > 50 ? trimmedName.Substring(0, 50) : trimmedName;
        var request = new CreateRoleRequest(validName, "Description", hierarchyLevel.Get);
        var result = validator.Validate(request);
        
        // Should not have name-related required field errors
        return !result.Errors.Any(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("必填"));
    }

    #endregion

    #region Property 2.3: 使用者請求必填欄位驗證 (需求 4.7)

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白使用者帳號應被拒絕
    /// **Validates: Requirements 4.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateUserRequest_EmptyAccount_ShouldBeRejected(PositiveInt seed)
    {
        var validator = new CreateUserRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        var validPassword = GenerateValidCredential(seed.Get);
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateUserRequest(emptyValue!, validPassword, "DisplayName", null);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Account" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白使用者密碼應被拒絕
    /// **Validates: Requirements 4.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateUserRequest_EmptyPassword_ShouldBeRejected(PositiveInt seed)
    {
        var validator = new CreateUserRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        var validAccount = GenerateValidCredential(seed.Get);
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateUserRequest(validAccount, emptyValue!, "DisplayName", null);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白顯示名稱應被拒絕
    /// **Validates: Requirements 4.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateUserRequest_EmptyDisplayName_ShouldBeRejected(PositiveInt seed)
    {
        var validator = new CreateUserRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        var validAccount = GenerateValidCredential(seed.Get);
        var validPassword = GenerateValidCredential(seed.Get + 10000);
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateUserRequest(validAccount, validPassword, emptyValue!, null);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "DisplayName" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 所有使用者必填欄位都為空時應回傳所有錯誤
    /// **Validates: Requirements 4.7**
    /// </summary>
    [Fact]
    public void CreateUserRequest_AllEmptyFields_ShouldReturnAllErrors()
    {
        var validator = new CreateUserRequestValidator();
        var request = new CreateUserRequest("", "", "", null);
        var result = validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Account" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName" && e.ErrorMessage.Contains("必填"));
    }

    #endregion

    #region Property 2.4: 功能請求必填欄位驗證 (需求 5.4)

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白功能名稱應被拒絕
    /// **Validates: Requirements 5.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateFunctionRequest_EmptyName_ShouldBeRejected(NonEmptyString code)
    {
        var validator = new CreateFunctionRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateFunctionRequest(emptyValue!, code.Get, "/url", false, "icon", null, 0);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白功能代碼應被拒絕
    /// **Validates: Requirements 5.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateFunctionRequest_EmptyCode_ShouldBeRejected(NonEmptyString name)
    {
        var validator = new CreateFunctionRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateFunctionRequest(name.Get, emptyValue!, "/url", false, "icon", null, 0);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 所有功能必填欄位都為空時應回傳所有錯誤
    /// **Validates: Requirements 5.4**
    /// </summary>
    [Fact]
    public void CreateFunctionRequest_AllEmptyFields_ShouldReturnAllErrors()
    {
        var validator = new CreateFunctionRequestValidator();
        var request = new CreateFunctionRequest("", "", "/url", false, "icon", null, 0);
        var result = validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("必填"));
    }

    #endregion

    #region Property 2.5: 分類請求必填欄位驗證 (需求 6.4)

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白分類名稱應被拒絕
    /// **Validates: Requirements 6.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateCategoryRequest_EmptyName_ShouldBeRejected(NonEmptyString slug)
    {
        var validator = new CreateCategoryRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateCategoryRequest(emptyValue!, slug.Get, null, null, null, null, 0);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白分類網址代稱應被拒絕
    /// **Validates: Requirements 6.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateCategoryRequest_EmptySlug_ShouldBeRejected(NonEmptyString name)
    {
        var validator = new CreateCategoryRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        var validName = name.Get.Length > 20 ? name.Get.Substring(0, 20) : name.Get;
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateCategoryRequest(validName, emptyValue!, null, null, null, null, 0);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Slug" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 所有分類必填欄位都為空時應回傳所有錯誤
    /// **Validates: Requirements 6.4**
    /// </summary>
    [Fact]
    public void CreateCategoryRequest_AllEmptyFields_ShouldReturnAllErrors()
    {
        var validator = new CreateCategoryRequestValidator();
        var request = new CreateCategoryRequest("", "", null, null, null, null, 0);
        var result = validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "Slug" && e.ErrorMessage.Contains("必填"));
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - CategoryNameValidator 空白名稱應被拒絕
    /// **Validates: Requirements 6.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryNameValidator_EmptyName_ShouldBeRejected(PositiveInt seed)
    {
        var validator = new CategoryNameValidator();
        // Note: FluentValidation throws ArgumentNullException for null, so we test empty/whitespace strings
        var emptyValues = new[] { "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var result = validator.Validate(emptyValue);
            
            if (result.IsValid || !result.Errors.Any(e => e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    #endregion

    #region Property 2.6: 文章請求必填欄位驗證 (需求 7.6)

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白文章標題應被拒絕
    /// **Validates: Requirements 7.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateArticleRequest_EmptyTitle_ShouldBeRejected(NonEmptyString content, NonEmptyString slug, PositiveInt categoryId)
    {
        var validator = new CreateArticleRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateArticleRequest(emptyValue!, content.Get, slug.Get, categoryId.Get, null, null, null, null);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白文章內容應被拒絕
    /// **Validates: Requirements 7.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateArticleRequest_EmptyContent_ShouldBeRejected(NonEmptyString title, NonEmptyString slug, PositiveInt categoryId)
    {
        var validator = new CreateArticleRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        var validTitle = title.Get.Length > 200 ? title.Get.Substring(0, 200) : title.Get;
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateArticleRequest(validTitle, emptyValue!, slug.Get, categoryId.Get, null, null, null, null);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Content" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 空白文章網址代稱應被拒絕
    /// **Validates: Requirements 7.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateArticleRequest_EmptySlug_ShouldBeRejected(NonEmptyString title, NonEmptyString content, PositiveInt categoryId)
    {
        var validator = new CreateArticleRequestValidator();
        var emptyValues = new[] { null, "", "   ", "\t", "\n" };
        var validTitle = title.Get.Length > 200 ? title.Get.Substring(0, 200) : title.Get;
        
        foreach (var emptyValue in emptyValues)
        {
            var request = new CreateArticleRequest(validTitle, content.Get, emptyValue!, categoryId.Get, null, null, null, null);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "Slug" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 無效分類 ID 應被拒絕
    /// **Validates: Requirements 7.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CreateArticleRequest_InvalidCategoryId_ShouldBeRejected(NonEmptyString title, NonEmptyString content, NonEmptyString slug)
    {
        var validator = new CreateArticleRequestValidator();
        var invalidCategoryIds = new[] { 0, -1, -100 };
        var validTitle = title.Get.Length > 200 ? title.Get.Substring(0, 200) : title.Get;
        
        foreach (var invalidId in invalidCategoryIds)
        {
            var request = new CreateArticleRequest(validTitle, content.Get, slug.Get, invalidId, null, null, null, null);
            var result = validator.Validate(request);
            
            if (result.IsValid || !result.Errors.Any(e => e.PropertyName == "CategoryId" && e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 所有文章必填欄位都為空時應回傳所有錯誤
    /// **Validates: Requirements 7.6**
    /// </summary>
    [Fact]
    public void CreateArticleRequest_AllEmptyFields_ShouldReturnAllErrors()
    {
        var validator = new CreateArticleRequestValidator();
        var request = new CreateArticleRequest("", "", "", 0, null, null, null, null);
        var result = validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "Content" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "Slug" && e.ErrorMessage.Contains("必填"));
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId" && e.ErrorMessage.Contains("必填"));
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - ArticleTitleValidator 空白標題應被拒絕
    /// **Validates: Requirements 7.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleTitleValidator_EmptyTitle_ShouldBeRejected(PositiveInt seed)
    {
        var validator = new ArticleTitleValidator();
        // Note: FluentValidation throws ArgumentNullException for null, so we test empty/whitespace strings
        var emptyValues = new[] { "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var result = validator.Validate(emptyValue);
            
            if (result.IsValid || !result.Errors.Any(e => e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    #endregion

    #region Property 2.7: 帳號密碼必填欄位驗證 (需求 1.2)

    /// <summary>
    /// Property 2: 必填欄位驗證 - AccountValidator 空白帳號應被拒絕
    /// **Validates: Requirements 1.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AccountValidator_EmptyAccount_ShouldBeRejected(PositiveInt seed)
    {
        var validator = new AccountValidator();
        // Note: FluentValidation throws ArgumentNullException for null, so we test empty/whitespace strings
        var emptyValues = new[] { "", "   ", "\t", "\n" };
        
        foreach (var emptyValue in emptyValues)
        {
            var result = validator.Validate(emptyValue);
            
            if (result.IsValid || !result.Errors.Any(e => e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - PasswordValidator 空白密碼應被拒絕
    /// **Validates: Requirements 1.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PasswordValidator_EmptyPassword_ShouldBeRejected(PositiveInt seed)
    {
        var validator = new PasswordValidator();
        // Note: FluentValidation throws ArgumentNullException for null, so we test empty/whitespace strings
        var emptyValues = new[] { "", "   ", "\t", "\n" };
        var validAccount = GenerateValidCredential(seed.Get);
        
        foreach (var emptyValue in emptyValues)
        {
            var context = new PasswordValidationContext(emptyValue, validAccount);
            var result = validator.Validate(context);
            
            if (result.IsValid || !result.Errors.Any(e => e.ErrorMessage.Contains("必填")))
                return false;
        }
        return true;
    }

    #endregion

    #region Property 2.8: 通用必填欄位驗證屬性

    /// <summary>
    /// Property 2: 必填欄位驗證 - 任意空白字串組合應被所有驗證器拒絕
    /// **Validates: Requirements 1.2, 2.2, 4.7, 5.4, 6.4, 7.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AllValidators_EmptyStrings_ShouldBeRejected(PositiveInt seed)
    {
        var random = new Random(seed.Get);
        // Note: FluentValidation throws ArgumentNullException for null, so we test empty/whitespace strings
        var emptyValues = new[] { "", "   ", "\t", "\n", "  \t  ", "\r\n" };
        var emptyValue = emptyValues[random.Next(emptyValues.Length)];
        
        // Test AccountValidator
        var accountValidator = new AccountValidator();
        var accountResult = accountValidator.Validate(emptyValue);
        if (accountResult.IsValid) return false;
        
        // Test CategoryNameValidator
        var categoryValidator = new CategoryNameValidator();
        var categoryResult = categoryValidator.Validate(emptyValue);
        if (categoryResult.IsValid) return false;
        
        // Test ArticleTitleValidator
        var articleValidator = new ArticleTitleValidator();
        var articleResult = articleValidator.Validate(emptyValue);
        if (articleResult.IsValid) return false;
        
        return true;
    }

    /// <summary>
    /// Property 2: 必填欄位驗證 - 非空白字串應通過必填驗證（不考慮其他規則）
    /// **Validates: Requirements 1.2, 2.2, 4.7, 5.4, 6.4, 7.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AllValidators_NonEmptyStrings_ShouldPassRequiredCheck(NonEmptyString value)
    {
        // Trim to ensure we're testing actual non-empty content
        var trimmedValue = value.Get.Trim();
        if (string.IsNullOrWhiteSpace(trimmedValue))
            return true; // Skip whitespace-only strings
        
        // Test CategoryNameValidator (with length constraint)
        var categoryValidator = new CategoryNameValidator();
        var categoryValue = trimmedValue.Length > 20 ? trimmedValue.Substring(0, 20) : trimmedValue;
        var categoryResult = categoryValidator.Validate(categoryValue);
        // Should not have "必填" error
        if (categoryResult.Errors.Any(e => e.ErrorMessage.Contains("必填")))
            return false;
        
        // Test ArticleTitleValidator (with length constraint)
        var articleValidator = new ArticleTitleValidator();
        var articleValue = trimmedValue.Length > 200 ? trimmedValue.Substring(0, 200) : trimmedValue;
        var articleResult = articleValidator.Validate(articleValue);
        // Should not have "必填" error
        if (articleResult.Errors.Any(e => e.ErrorMessage.Contains("必填")))
            return false;
        
        return true;
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

    private static void Shuffle(char[] array, Random random)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    #endregion
}
