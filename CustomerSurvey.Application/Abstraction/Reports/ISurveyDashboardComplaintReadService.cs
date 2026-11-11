using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Domain.Enums;

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
    int PageSize,
    SurveyResponseFlagType FlagType = SurveyResponseFlagType.Complaint,
    bool? Answered = null,
    string? SearchText = null,
    Guid? FlagId = null);

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
    [System.Text.Json.Serialization.JsonIgnore]
    public string? SnapshotPayload { get; init; }
    public Guid ComplaintId { get; init; }
    public SurveyDashboardSource Source { get; init; }
    public Guid ResponseId { get; init; }
    public Guid TemplateId { get; init; }
    public SurveyDashboardTemplateKind TemplateKind { get; init; }
    public string TemplateNameEn { get; init; } = string.Empty;
    public string? TemplateNameAr { get; init; }
    public Guid? QuestionId { get; init; }
    public Guid? FlagId { get; init; }
    public SurveyResponseFlagType? FlagType { get; init; }
    public string? Brief { get; init; }
    public string? Answer { get; init; }
    public Guid? CreatedById { get; init; }
    public string? CreatedByNameEn { get; init; }
    public string? CreatedByNameAr { get; init; }
    public DateTime? CreatedOnUtc { get; init; }
    public Guid? BriefUpdatedById { get; init; }
    public string? BriefUpdatedByNameEn { get; init; }
    public string? BriefUpdatedByNameAr { get; init; }
    public DateTime? BriefUpdatedOnUtc { get; init; }
    public Guid? AnsweredById { get; init; }
    public string? AnsweredByNameEn { get; init; }
    public string? AnsweredByNameAr { get; init; }
    public DateTime? AnsweredOnUtc { get; init; }
    public string QuestionTextEn { get; init; } = string.Empty;
    public string? QuestionTextAr { get; init; }
    public string? ComplaintText { get; init; }
    public DateTime SubmittedOnUtc { get; init; }
    public Guid BranchId { get; init; }
    public string BranchNameEn { get; init; } = string.Empty;
    public string? BranchNameAr { get; init; }
    public Guid? OperatorId { get; init; }
    public string? OperatorNameEn { get; init; }
    public string? OperatorNameAr { get; init; }
}
