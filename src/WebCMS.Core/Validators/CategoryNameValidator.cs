using FluentValidation;

namespace WebCMS.Core.Validators;

/// <summary>
/// 分類名稱驗證器
/// 規則：必填，最多 20 字元
/// 驗證: 需求 6.3, 6.4
/// </summary>
public class CategoryNameValidator : AbstractValidator<string>
{
    public CategoryNameValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("分類名稱為必填欄位")
            .MaximumLength(20).WithMessage("分類名稱最多 20 字元");
    }
}
