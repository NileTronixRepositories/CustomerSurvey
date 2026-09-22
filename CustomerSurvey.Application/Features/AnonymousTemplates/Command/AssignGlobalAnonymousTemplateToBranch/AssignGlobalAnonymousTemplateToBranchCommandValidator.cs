using CustomerSurvey.Application.Shared.Media;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignGlobalAnonymousTemplateToBranch;

internal sealed class AssignGlobalAnonymousTemplateToBranchCommandValidator
    : AbstractValidator<AssignGlobalAnonymousTemplateToBranchCommand>
{
    public AssignGlobalAnonymousTemplateToBranchCommandValidator()
    {
        RuleFor(x => x.GlobalTemplateId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.ActiveFrom).NotEmpty();
        RuleFor(x => x.ExpireTo)
            .GreaterThan(x => x.ActiveFrom)
            .When(x => x.ExpireTo.HasValue);
        RuleFor(x => x.Logo).Custom((logo, context) =>
        {
            if (logo is null)
            {
                return;
            }

            var error = TemplateLogoMedia.Validate(logo);
            if (error is not null)
            {
                context.AddFailure(nameof(AssignGlobalAnonymousTemplateToBranchCommand.Logo), error.Message);
            }
        });
    }
}
