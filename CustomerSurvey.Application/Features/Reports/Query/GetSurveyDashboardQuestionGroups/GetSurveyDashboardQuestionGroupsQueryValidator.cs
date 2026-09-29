using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardQuestionGroups;

internal sealed class GetSurveyDashboardQuestionGroupsQueryValidator
    : AbstractValidator<GetSurveyDashboardQuestionGroupsQuery>
{
    public GetSurveyDashboardQuestionGroupsQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value.Date <= x.To.Value.Date)
            .WithMessage(ErrorMessage.GetSurveyDashboard_DateRange_Invalid);

        RuleFor(x => x.Source).IsInEnum();
        RuleFor(x => x)
            .Must(x => !x.TemplateId.HasValue || !x.AnonymousTemplateId.HasValue)
            .WithMessage(ErrorMessage.SurveyDashboard_TemplateFilter_Ambiguous);
        RuleFor(x => x.ScoreCalculationMode)
            .IsInEnum()
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_ScoreCalculationMode_Invalid);
    }
}
