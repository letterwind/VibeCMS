using FluentValidation;
using WebCMS.Core.DTOs.Auth;

namespace WebCMS.Core.Validators;

/// <summary>
/// 登入請求驗證器
/// 驗證: 需求 1.2
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Account)
            .NotEmpty().WithMessage("帳號為必填欄位");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼為必填欄位");

        RuleFor(x => x.Captcha)
            .NotEmpty().WithMessage("驗證碼為必填欄位");

        RuleFor(x => x.CaptchaToken)
            .NotEmpty().WithMessage("驗證碼 Token 為必填欄位");
    }
}
