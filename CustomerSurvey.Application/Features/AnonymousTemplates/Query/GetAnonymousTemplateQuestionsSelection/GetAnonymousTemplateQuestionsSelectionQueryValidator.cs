using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed class GetAnonymousTemplateQuestionsSelectionQueryValidator
        : AbstractValidator<GetAnonymousTemplateQuestionsSelectionQuery>
    {
        public GetAnonymousTemplateQuestionsSelectionQueryValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetAnonymousTemplateQuestionsSelection_AnonymousTemplateId_Required);

            RuleFor(x => x.SearchText)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.GetAnonymousTemplateQuestionsSelection_SearchText_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
        }
    }
}