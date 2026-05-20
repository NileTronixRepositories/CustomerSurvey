using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed class GetDepartmentDashboardQueryValidator
    : AbstractValidator<GetDepartmentDashboardQuery>
{
    public GetDepartmentDashboardQueryValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !x.From.HasValue ||
                !x.To.HasValue ||
                x.From.Value <= x.To.Value)
            .WithMessage(ErrorMessage.GetDepartmentDashboard_DateRange_Invalid);

        RuleFor(x => x.TopQuestionsCount)
            .InclusiveBetween(1, 20)
            .WithMessage(ErrorMessage.GetDepartmentDashboard_TopQuestionsCount_Invalid);

        RuleFor(x => x.CriticalResponsesCount)
            .InclusiveBetween(1, 50)
            .WithMessage(ErrorMessage.GetDepartmentDashboard_CriticalResponsesCount_Invalid);

        RuleFor(x => x.CriticalScoreThreshold)
            .InclusiveBetween(0m, 100m)
            .WithMessage(ErrorMessage.GetDepartmentDashboard_CriticalScoreThreshold_Invalid);

        RuleFor(x => x.GroupBy)
            .IsInEnum()
            .WithMessage(ErrorMessage.GetDepartmentDashboard_GroupBy_Invalid);
    }
}
