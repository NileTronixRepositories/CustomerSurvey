using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Shared;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyResponsesPagination;

public sealed record SurveyResponsePaginationItemResponse
{
    public SurveyDashboardSource Source { get; init; }

    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public Guid? OperatorId { get; init; }

    public string? OperatorNameEn { get; init; }

    public string? OperatorNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public decimal ScorePercentage { get; init; }

    public bool IsScored { get; init; }

    public bool HasComplaint { get; init; }

    public bool HasVoice { get; init; }

    public IReadOnlyCollection<SurveyResponseCustomInputPreviewResponse> CustomInputsPreview { get; init; }
        = Array.Empty<SurveyResponseCustomInputPreviewResponse>();

    public DashboardDetailsNavigationResponse DetailsNavigation { get; init; } = new();
}

public sealed record SurveyResponseCustomInputPreviewResponse
{
    public string Name { get; init; } = string.Empty;

    public string? LabelEn { get; init; }

    public string? LabelAr { get; init; }

    public string Value { get; init; } = string.Empty;
}
