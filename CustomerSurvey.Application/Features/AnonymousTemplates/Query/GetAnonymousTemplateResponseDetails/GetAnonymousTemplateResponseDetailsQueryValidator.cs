using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetAnonymousTemplateResponseDetailsQueryValidator
        : AbstractValidator<GetAnonymousTemplateResponseDetailsQuery>
    {
        public GetAnonymousTemplateResponseDetailsQueryValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponseDetails_AnonymousTemplateId_Required);

            RuleFor(x => x.ResponseId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponseDetails_ResponseId_Required);
        }
    }
}