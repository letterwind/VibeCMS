using FluentValidation;
using WebCMS.Core.DTOs.User;

namespace WebCMS.Core.Validators;

/// <summary>
/// 建立使用者請求驗證器
/// 驗證: 需求 4.2, 4.3, 4.4, 4.7
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // 帳號驗證
        RuleFor(x => x.Account)
            .NotEmpty().WithMessage("帳號為必填欄位")
            .Length(6, 12).WithMessage("帳號長度必須為 6-12 字元")
            .Matches(@"[A-Z]").WithMessage("帳號必須包含至少 1 個大寫字母")
            .Matches(@"[a-z]").WithMessage("帳號必須包含至少 1 個小寫字母")
            .Matches(@"[0-9]").WithMessage("帳號必須包含至少 1 個數字");

        // 密碼驗證
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼為必填欄位")
            .Length(6, 12).WithMessage("密碼長度必須為 6-12 字元")
            .Matches(@"[A-Z]").WithMessage("密碼必須包含至少 1 個大寫字母")
            .Matches(@"[a-z]").WithMessage("密碼必須包含至少 1 個小寫字母")
            .Matches(@"[0-9]").WithMessage("密碼必須包含至少 1 個數字");

        // 密碼不得包含帳號
        RuleFor(x => x)
            .Must(x => !x.Password.Contains(x.Account, StringComparison.OrdinalIgnoreCase))
            .WithMessage("密碼不得包含帳號內容")
            .When(x => !string.IsNullOrEmpty(x.Password) && !string.IsNullOrEmpty(x.Account));

        // 顯示名稱驗證
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("顯示名稱為必填欄位")
            .MaximumLength(100).WithMessage("顯示名稱最多 100 字元");
    }
}
