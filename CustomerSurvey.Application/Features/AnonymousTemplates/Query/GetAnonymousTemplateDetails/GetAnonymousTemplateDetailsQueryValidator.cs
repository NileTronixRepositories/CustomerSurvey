using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetAnonymousTemplateDetailsQueryValidator
        : AbstractValidator<GetAnonymousTemplateDetailsQuery>
    {
        public GetAnonymousTemplateDetailsQueryValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetAnonymousTemplateDetails_AnonymousTemplateId_Required);
        }
    }
}