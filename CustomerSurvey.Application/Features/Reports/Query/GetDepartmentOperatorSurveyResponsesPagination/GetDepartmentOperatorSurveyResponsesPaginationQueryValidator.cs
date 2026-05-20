using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

internal sealed class GetDepartmentOperatorSurveyResponsesPaginationQueryValidator
    : AbstractValidator<GetDepartmentOperatorSurveyResponsesPaginationQuery>
{
    public GetDepartmentOperatorSurveyResponsesPaginationQueryValidator()
    {
        RuleFor(x => x.OperatorId)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_OperatorId_Required);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_PageSize_Invalid);

        RuleFor(x => x)
            .Must(x =>
                !x.From.HasValue ||
                !x.To.HasValue ||
                x.From.Value <= x.To.Value)
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_DateRange_Invalid);

        RuleFor(x => x.MinScorePercentage)
            .InclusiveBetween(0m, 100m)
            .When(x => x.MinScorePercentage.HasValue)
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_MinScore_Invalid);

        RuleFor(x => x.MaxScorePercentage)
            .InclusiveBetween(0m, 100m)
            .When(x => x.MaxScorePercentage.HasValue)
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_MaxScore_Invalid);

        RuleFor(x => x)
            .Must(x =>
                !x.MinScorePercentage.HasValue ||
                !x.MaxScorePercentage.HasValue ||
                x.MinScorePercentage.Value <= x.MaxScorePercentage.Value)
            .WithMessage(ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_ScoreRange_Invalid);
    }
}
