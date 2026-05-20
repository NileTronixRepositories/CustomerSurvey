using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

internal sealed class GetSystemSurveyResponseDetailsQueryValidator
    : AbstractValidator<GetSystemSurveyResponseDetailsQuery>
{
    public GetSystemSurveyResponseDetailsQueryValidator()
    {
        RuleFor(x => x.SurveyResponseId)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetSystemSurveyResponseDetails_SurveyResponseId_Required);
    }
}