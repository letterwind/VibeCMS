using FluentValidation;

namespace WebCMS.Core.Validators;

/// <summary>
/// 帳號驗證器
/// 規則：6-12 字元，必須包含至少 1 個大寫字母、1 個小寫字母、1 個數字
/// 驗證: 需求 4.2
/// </summary>
public class AccountValidator : AbstractValidator<string>
{
    public AccountValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("帳號為必填欄位")
            .Length(6, 12).WithMessage("帳號長度必須為 6-12 字元")
            .Matches(@"[A-Z]").WithMessage("帳號必須包含至少 1 個大寫字母")
            .Matches(@"[a-z]").WithMessage("帳號必須包含至少 1 個小寫字母")
            .Matches(@"[0-9]").WithMessage("帳號必須包含至少 1 個數字");
    }
}
