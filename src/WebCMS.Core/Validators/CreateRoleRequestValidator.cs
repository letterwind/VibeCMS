using FluentValidation;
using WebCMS.Core.DTOs.Role;

namespace WebCMS.Core.Validators;

/// <summary>
/// 建立角色請求驗證器
/// 驗證: 需求 2.2
/// </summary>
public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("角色名稱為必填欄位")
            .MaximumLength(50).WithMessage("角色名稱最多 50 字元");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("角色描述最多 200 字元")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.HierarchyLevel)
            .GreaterThan(0).WithMessage("階層等級必須大於 0");
    }
}
