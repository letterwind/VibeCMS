using FluentValidation;
using WebCMS.Core.DTOs.Category;

namespace WebCMS.Core.Validators;

/// <summary>
/// 建立分類請求驗證器
/// 驗證: 需求 6.3, 6.4
/// </summary>
public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("分類名稱為必填欄位")
            .MaximumLength(20).WithMessage("分類名稱最多 20 字元");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("網址代稱為必填欄位")
            .MaximumLength(100).WithMessage("網址代稱最多 100 字元");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(100).WithMessage("SEO 標題最多 100 字元")
            .When(x => !string.IsNullOrEmpty(x.MetaTitle));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(200).WithMessage("SEO 描述最多 200 字元")
            .When(x => !string.IsNullOrEmpty(x.MetaDescription));

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(200).WithMessage("SEO 關鍵字最多 200 字元")
            .When(x => !string.IsNullOrEmpty(x.MetaKeywords));
    }
}
