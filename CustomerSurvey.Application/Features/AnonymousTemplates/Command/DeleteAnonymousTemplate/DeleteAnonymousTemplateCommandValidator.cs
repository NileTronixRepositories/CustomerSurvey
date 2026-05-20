using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplate
{
    internal sealed class DeleteAnonymousTemplateCommandValidator
        : AbstractValidator<DeleteAnonymousTemplateCommand>
    {
        public DeleteAnonymousTemplateCommandValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeleteAnonymousTemplate_AnonymousTemplateId_Required);
        }
    }
}