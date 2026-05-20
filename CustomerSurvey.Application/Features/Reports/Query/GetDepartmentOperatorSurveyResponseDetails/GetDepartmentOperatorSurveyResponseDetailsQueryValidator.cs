using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed class GetDepartmentOperatorSurveyResponseDetailsQueryValidator
    : AbstractValidator<GetDepartmentOperatorSurveyResponseDetailsQuery>
{
    public GetDepartmentOperatorSurveyResponseDetailsQueryValidator()
    {
        RuleFor(x => x.OperatorId)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponseDetails_OperatorId_Required);

        RuleFor(x => x.SurveyResponseId)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponseDetails_SurveyResponseId_Required);
    }
}
