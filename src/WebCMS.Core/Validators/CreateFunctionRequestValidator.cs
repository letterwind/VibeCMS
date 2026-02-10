using FluentValidation;
using WebCMS.Core.DTOs.Function;

namespace WebCMS.Core.Validators;

/// <summary>
/// 建立功能請求驗證器
/// 驗證: 需求 5.4
/// </summary>
public class CreateFunctionRequestValidator : AbstractValidator<CreateFunctionRequest>
{
    public CreateFunctionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("功能名稱為必填欄位")
            .MaximumLength(100).WithMessage("功能名稱最多 100 字元");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("功能代碼為必填欄位")
            .MaximumLength(50).WithMessage("功能代碼最多 50 字元");

        RuleFor(x => x.Url)
            .MaximumLength(500).WithMessage("連結網址最多 500 字元")
            .When(x => !string.IsNullOrEmpty(x.Url));

        RuleFor(x => x.Icon)
            .MaximumLength(100).WithMessage("圖示名稱最多 100 字元")
            .When(x => !string.IsNullOrEmpty(x.Icon));
    }
}
