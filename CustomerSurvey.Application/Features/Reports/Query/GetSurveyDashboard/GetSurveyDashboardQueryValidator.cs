using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal sealed class GetSurveyDashboardQueryValidator
    : AbstractValidator<GetSurveyDashboardQuery>
{
    public GetSurveyDashboardQueryValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !x.From.HasValue ||
                !x.To.HasValue ||
                x.From.Value.Date <= x.To.Value.Date)
            .WithMessage(ErrorMessage.GetSurveyDashboard_DateRange_Invalid);

        RuleFor(x => x.TopQuestionsCount)
            .InclusiveBetween(1, 50)
            .WithMessage(ErrorMessage.GetSurveyDashboard_TopQuestionsCount_Invalid);

        RuleFor(x => x.CriticalResponsesCount)
            .InclusiveBetween(1, 100)
            .WithMessage(ErrorMessage.GetSurveyDashboard_CriticalResponsesCount_Invalid);

        RuleFor(x => x.CriticalScoreThreshold)
            .InclusiveBetween(0m, 100m)
            .WithMessage(ErrorMessage.GetSurveyDashboard_CriticalScoreThreshold_Invalid);

        RuleFor(x => x.ScoreCalculationMode)
            .IsInEnum()
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_ScoreCalculationMode_Invalid);
    }
}
