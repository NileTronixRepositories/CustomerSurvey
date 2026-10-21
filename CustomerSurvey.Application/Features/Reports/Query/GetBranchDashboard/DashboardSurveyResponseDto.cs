namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed record DashboardSurveyResponseDto : CustomerSurvey.Application.Features.Reports.Shared.IEvaluatedResponse
{
    public string? SnapshotPayload { get; init; }
    public CustomerSurvey.Application.Features.Reports.Shared.EvaluationCriteria Criteria { get; init; } = new();
    public bool RequestedModeAvailable { get; init; } = true;
    public string ScoreDerivation { get; init; } = "SubmissionSnapshot";

    public Guid SurveyResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public int ActualScore { get; init; }

    public int MaxScore { get; init; }

    public decimal ScorePercentage { get; init; }
}