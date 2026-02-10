using FluentValidation;

namespace WebCMS.Core.Validators;

/// <summary>
/// 文章標題驗證器
/// 規則：必填，最多 200 字元
/// 驗證: 需求 7.2, 7.3, 7.6
/// </summary>
public class ArticleTitleValidator : AbstractValidator<string>
{
    public ArticleTitleValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("文章標題為必填欄位")
            .MaximumLength(200).WithMessage("文章標題最多 200 字元");
    }
}
