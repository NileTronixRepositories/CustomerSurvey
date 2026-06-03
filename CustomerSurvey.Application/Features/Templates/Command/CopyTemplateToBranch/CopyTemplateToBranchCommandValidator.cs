using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

internal sealed class CopyTemplateToBranchCommandValidator
    : AbstractValidator<CopyTemplateToBranchCommand>
{
    public CopyTemplateToBranchCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage(ErrorMessage.CopyTemplateToBranch_TemplateId_Required);

        RuleFor(x => x.BranchId)
            .NotEmpty()
            .WithMessage(ErrorMessage.CopyTemplateToBranch_BranchId_Required);
    }
}
