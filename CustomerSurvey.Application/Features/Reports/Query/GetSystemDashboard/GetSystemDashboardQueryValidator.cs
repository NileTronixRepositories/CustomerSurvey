using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed class GetSystemDashboardQueryValidator
    : AbstractValidator<GetSystemDashboardQuery>
{
    public GetSystemDashboardQueryValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !x.From.HasValue ||
                !x.To.HasValue ||
                x.From.Value <= x.To.Value)
            .WithMessage(ErrorMessage.GetSystemDashboard_DateRange_Invalid);

        RuleFor(x => x.GroupBy)
            .IsInEnum()
            .WithMessage(ErrorMessage.GetSystemDashboard_GroupBy_Invalid);

        RuleFor(x => x.CriticalScoreThreshold)
            .InclusiveBetween(0m, 100m)
            .WithMessage(ErrorMessage.GetSystemDashboard_CriticalScoreThreshold_Invalid);

        RuleFor(x => x.CriticalResponsesCount)
            .InclusiveBetween(1, 50)
            .WithMessage(ErrorMessage.GetSystemDashboard_CriticalResponsesCount_Invalid);

        RuleFor(x => x.TopTemplatesCount)
            .InclusiveBetween(1, 50)
            .WithMessage(ErrorMessage.GetSystemDashboard_TopTemplatesCount_Invalid);
    }
}