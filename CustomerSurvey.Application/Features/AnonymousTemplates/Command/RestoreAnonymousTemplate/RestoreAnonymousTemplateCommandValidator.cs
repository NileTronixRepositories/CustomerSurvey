using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.RestoreAnonymousTemplate
{
    internal sealed class RestoreAnonymousTemplateCommandValidator
        : AbstractValidator<RestoreAnonymousTemplateCommand>
    {
        public RestoreAnonymousTemplateCommandValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreAnonymousTemplate_AnonymousTemplateId_Required);
        }
    }
}