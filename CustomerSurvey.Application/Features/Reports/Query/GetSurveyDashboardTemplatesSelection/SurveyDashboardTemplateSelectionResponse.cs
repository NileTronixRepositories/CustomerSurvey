using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardTemplatesSelection;

public sealed record SurveyDashboardTemplateSelectionResponse
{
    public Guid TemplateId { get; init; }

    public SurveyDashboardTemplateKind TemplateKind { get; init; }

    public SurveyDashboardSource DashboardSource { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public string BranchCode { get; init; } = string.Empty;
}
