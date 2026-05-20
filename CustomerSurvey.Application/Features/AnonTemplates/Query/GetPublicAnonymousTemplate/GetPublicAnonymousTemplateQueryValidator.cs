using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate
{
    internal sealed class GetPublicAnonymousTemplateQueryValidator
        : AbstractValidator<GetPublicAnonymousTemplateQuery>
    {
        public GetPublicAnonymousTemplateQueryValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetPublicAnonymousTemplate_AnonymousTemplateId_Required);
        }
    }
}