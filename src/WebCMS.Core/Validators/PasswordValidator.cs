using FluentValidation;

namespace WebCMS.Core.Validators;

/// <summary>
/// 密碼驗證器
/// 規則：6-12 字元，必須包含至少 1 個大寫字母、1 個小寫字母、1 個數字
/// 額外規則：密碼不得包含帳號內容
/// 驗證: 需求 4.3, 4.4
/// </summary>
public class PasswordValidator : AbstractValidator<PasswordValidationContext>
{
    public PasswordValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼為必填欄位")
            .Length(6, 12).WithMessage("密碼長度必須為 6-12 字元")
            .Matches(@"[A-Z]").WithMessage("密碼必須包含至少 1 個大寫字母")
            .Matches(@"[a-z]").WithMessage("密碼必須包含至少 1 個小寫字母")
            .Matches(@"[0-9]").WithMessage("密碼必須包含至少 1 個數字");

        RuleFor(x => x)
            .Must(ctx => !ContainsAccount(ctx.Password, ctx.Account))
            .WithMessage("密碼不得包含帳號內容")
            .When(x => !string.IsNullOrEmpty(x.Password) && !string.IsNullOrEmpty(x.Account));
    }

    private static bool ContainsAccount(string password, string account)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(account))
            return false;

        return password.Contains(account, StringComparison.OrdinalIgnoreCase);
    }
}
