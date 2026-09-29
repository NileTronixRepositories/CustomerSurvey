using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardComplaints;

public sealed record SurveyDashboardComplaintsResponse
{
    public SurveyDashboardAppliedFiltersResponse AppliedFilters { get; init; } = new();
    public int TotalComplaints { get; init; }
    public int ResponsesWithComplaints { get; init; }
    public int TotalResponses { get; init; }
    public decimal ComplaintRate { get; init; }
    public Pagination<SurveyDashboardComplaintItemResponse> Page { get; init; }
        = new(1, 10, 0, Array.Empty<SurveyDashboardComplaintItemResponse>());
    public IReadOnlyCollection<SurveyDashboardComplaintTemplateGroupResponse> TemplateGroups { get; init; }
        = Array.Empty<SurveyDashboardComplaintTemplateGroupResponse>();
}

public sealed record SurveyDashboardComplaintTemplateGroupResponse
{
    public Guid TemplateId { get; init; }
    public SurveyDashboardTemplateKind TemplateKind { get; init; }
    public string TemplateNameEn { get; init; } = string.Empty;
    public string? TemplateNameAr { get; init; }
    public Guid BranchId { get; init; }
    public string BranchNameEn { get; init; } = string.Empty;
    public string? BranchNameAr { get; init; }
    public int TotalComplaints { get; init; }
    public int ResponsesWithComplaints { get; init; }
    public IReadOnlyCollection<SurveyDashboardComplaintItemResponse> Complaints { get; init; }
        = Array.Empty<SurveyDashboardComplaintItemResponse>();
}

public sealed record SurveyDashboardComplaintItemResponse
{
    public Guid ResponseId { get; init; }
    public Guid TemplateId { get; init; }
    public SurveyDashboardTemplateKind TemplateKind { get; init; }
    public string TemplateNameEn { get; init; } = string.Empty;
    public string? TemplateNameAr { get; init; }
    public Guid QuestionId { get; init; }
    public string QuestionTextEn { get; init; } = string.Empty;
    public string? QuestionTextAr { get; init; }
    public string ComplaintText { get; init; } = string.Empty;
    public DateTime SubmittedOnUtc { get; init; }
    public Guid BranchId { get; init; }
    public string BranchNameEn { get; init; } = string.Empty;
    public string? BranchNameAr { get; init; }
    public SurveyDashboardSource Source { get; init; }
    public Guid? OperatorId { get; init; }
    public string? OperatorNameEn { get; init; }
    public string? OperatorNameAr { get; init; }
    public SurveyDashboardDetailsNavigationResponse DetailsNavigation { get; init; } = new();
}
