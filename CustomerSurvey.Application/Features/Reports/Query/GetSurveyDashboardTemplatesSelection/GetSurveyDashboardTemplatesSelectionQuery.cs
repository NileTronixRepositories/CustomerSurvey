using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardTemplatesSelection;

public sealed class GetSurveyDashboardTemplatesSelectionQuery
    : IQuery<IReadOnlyCollection<SurveyDashboardTemplateSelectionResponse>>
{
    public Guid? BranchId { get; init; }

    public string? SearchText { get; init; }

    public SurveyDashboardTemplateKind? TemplateKind { get; init; }
}
