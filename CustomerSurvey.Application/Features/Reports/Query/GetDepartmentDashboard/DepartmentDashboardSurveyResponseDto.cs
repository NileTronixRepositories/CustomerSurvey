namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed record DepartmentDashboardSurveyResponseDto : CustomerSurvey.Application.Features.Reports.Shared.IEvaluatedResponse
{
    public string? SnapshotPayload { get; init; }
    public CustomerSurvey.Application.Features.Reports.Shared.EvaluationCriteria Criteria { get; init; } = new();
    public bool RequestedModeAvailable { get; init; } = true;
    public string ScoreDerivation { get; init; } = "SubmissionSnapshot";

    public Guid SurveyResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public Guid OperatorId { get; init; }

    public string OperatorNameEn { get; init; } = string.Empty;

    public string? OperatorNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public DateOnly? VisitDate { get; init; }

    public int ActualScore { get; init; }

    public int ComplaintFlagsCount { get; init; }
    public int SuggestionsCount { get; init; }
    public int CommentsCount { get; init; }

    public int MaxScore { get; init; }

    public decimal ScorePercentage { get; init; }
}
