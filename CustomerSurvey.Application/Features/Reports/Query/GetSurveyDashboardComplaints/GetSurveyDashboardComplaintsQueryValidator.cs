using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardComplaints;

internal sealed class GetSurveyDashboardComplaintsQueryValidator
    : AbstractValidator<GetSurveyDashboardComplaintsQuery>
{
    public GetSurveyDashboardComplaintsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Source).IsInEnum();
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value.Date <= x.To.Value.Date)
            .WithMessage(ErrorMessage.GetSurveyDashboard_DateRange_Invalid);
        RuleFor(x => x)
            .Must(x => !x.TemplateId.HasValue || !x.AnonymousTemplateId.HasValue)
            .WithMessage(ErrorMessage.SurveyDashboard_TemplateFilter_Ambiguous);
    }
}
