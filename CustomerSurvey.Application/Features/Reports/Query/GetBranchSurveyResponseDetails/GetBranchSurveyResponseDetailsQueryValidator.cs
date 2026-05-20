using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed class GetBranchSurveyResponseDetailsQueryValidator
    : AbstractValidator<GetBranchSurveyResponseDetailsQuery>
{
    public GetBranchSurveyResponseDetailsQueryValidator()
    {
        RuleFor(x => x.SurveyResponseId)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetBranchSurveyResponseDetails_SurveyResponseId_Required);
    }
}