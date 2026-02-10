using FluentValidation;
using WebCMS.Core.DTOs.Article;

namespace WebCMS.Core.Validators;

/// <summary>
/// 建立文章請求驗證器
/// 驗證: 需求 7.2, 7.3, 7.6
/// </summary>
public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    public CreateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("文章標題為必填欄位")
            .MaximumLength(200).WithMessage("文章標題最多 200 字元");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("文章內容為必填欄位");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("網址代稱為必填欄位")
            .MaximumLength(200).WithMessage("網址代稱最多 200 字元");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("分類為必填欄位");

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
