using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

namespace CustomerSurvey.Application.Abstraction.Reports;

public interface ISurveyDashboardComplaintReadService
{
    Task<SurveyDashboardComplaintReadResult> ReadAsync(
        SurveyDashboardComplaintReadRequest request,
        CancellationToken cancellationToken);
}

public sealed record SurveyDashboardComplaintReadRequest(
    bool IncludeInternal,
    bool IncludeAnonymous,
    Guid? BranchId,
    Guid? InternalTemplateId,
    Guid? AnonymousTemplateId,
    DateTime FromUtc,
    DateTime ToExclusiveUtc,
    int PageNumber,
    int PageSize);

public sealed record SurveyDashboardComplaintReadResult(
    int TotalComplaints,
    int ResponsesWithComplaints,
    int TotalResponses,
    IReadOnlyCollection<SurveyDashboardComplaintTemplateReadGroup> TemplateGroups,
    IReadOnlyCollection<SurveyDashboardComplaintReadItem> PageItems);

public sealed record SurveyDashboardComplaintTemplateReadGroup
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
}

public sealed record SurveyDashboardComplaintReadItem
{
    public Guid ComplaintId { get; init; }
    public SurveyDashboardSource Source { get; init; }
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
    public Guid? OperatorId { get; init; }
    public string? OperatorNameEn { get; init; }
    public string? OperatorNameAr { get; init; }
}
