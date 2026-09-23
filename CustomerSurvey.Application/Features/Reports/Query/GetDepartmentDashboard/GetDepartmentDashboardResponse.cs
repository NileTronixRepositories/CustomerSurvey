using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

public sealed record GetDepartmentDashboardResponse
{
    public DepartmentDashboardPeriodResponse Period { get; init; } = new();

    public DepartmentDashboardSummaryResponse Summary { get; init; } = new();

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardChartsResponse Charts { get; init; } = new();

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardSummaryActionsResponse SummaryActions { get; init; } = new();

    public IReadOnlyCollection<DepartmentDashboardTrendPointResponse> SatisfactionTrend { get; init; }
        = Array.Empty<DepartmentDashboardTrendPointResponse>();

    public IReadOnlyCollection<DepartmentDashboardOperatorPerformanceResponse> OperatorPerformance { get; init; }
        = Array.Empty<DepartmentDashboardOperatorPerformanceResponse>();

    public IReadOnlyCollection<DepartmentDashboardTemplatePerformanceResponse> TemplatePerformance { get; init; }
        = Array.Empty<DepartmentDashboardTemplatePerformanceResponse>();

    public IReadOnlyCollection<DepartmentDashboardQuestionInsightResponse> LowestRatedQuestions { get; init; }
        = Array.Empty<DepartmentDashboardQuestionInsightResponse>();

    public IReadOnlyCollection<DepartmentDashboardCustomInputSegmentResponse> CustomInputSegments { get; init; }
        = Array.Empty<DepartmentDashboardCustomInputSegmentResponse>();

    public IReadOnlyCollection<DepartmentDashboardCriticalResponseItem> CriticalResponses { get; init; }
        = Array.Empty<DepartmentDashboardCriticalResponseItem>();
}

public sealed record DepartmentDashboardPeriodResponse
{
    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public bool IsDefaultPeriod { get; init; }

    public string GroupBy { get; init; } = string.Empty;
}

public sealed record DepartmentDashboardSummaryResponse
{
    public Guid DepartmentId { get; init; }

    public string DepartmentNameEn { get; init; } = string.Empty;

    public string? DepartmentNameAr { get; init; }

    public int TotalOperators { get; init; }

    public int ActiveOperators { get; init; }

    public int TotalAssignedTemplates { get; init; }

    public int ActiveAssignedTemplates { get; init; }

    public int TemplatesWithResponsesCount { get; init; }

    public int TotalResponses { get; init; }

    public int ScoredResponses { get; init; }

    public int UnscoredResponses { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int SatisfiedResponses { get; init; }

    public int NeutralResponses { get; init; }

    public int UnhappyResponses { get; init; }

    public int ComplaintsCount { get; init; }

    public int VoiceAnswersCount { get; init; }
}

public sealed record DepartmentDashboardTrendPointResponse
{
    public string Period { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record DepartmentDashboardOperatorPerformanceResponse
{
    public Guid OperatorId { get; init; }

    public string OperatorNameEn { get; init; } = string.Empty;

    public string? OperatorNameAr { get; init; }

    public bool IsActive { get; init; }

    public int ResponsesCount { get; init; }

    public int ScoredResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int ComplaintsCount { get; init; }

    public int VoiceAnswersCount { get; init; }

    public DateTime? LastResponseOnUtc { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record DepartmentDashboardTemplatePerformanceResponse
{
    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public bool IsActive { get; init; }

    public int ResponsesCount { get; init; }

    public int ScoredResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int ComplaintsCount { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record DepartmentDashboardQuestionInsightResponse
{
    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid QuestionId { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }

    public string QuestionTypeName { get; init; } = string.Empty;

    public int AnswersCount { get; init; }

    public decimal AverageValue { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record DepartmentDashboardCustomInputSegmentResponse
{
    public string CustomInputName { get; init; } = string.Empty;

    public TemplateCustomInputType Type { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public IReadOnlyCollection<DepartmentDashboardCustomInputSegmentValueResponse> Segments { get; init; }
        = Array.Empty<DepartmentDashboardCustomInputSegmentValueResponse>();
}

public sealed record DepartmentDashboardCustomInputSegmentValueResponse
{
    public string Value { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record DepartmentDashboardCriticalResponseItem
{
    public Guid SurveyResponseId { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid OperatorId { get; init; }

    public string OperatorNameEn { get; init; } = string.Empty;

    public string? OperatorNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public decimal ScorePercentage { get; init; }

    public string? ComplaintText { get; init; }

    public IReadOnlyCollection<DepartmentDashboardCriticalResponseCustomInputItem> CustomInputs { get; init; }
        = Array.Empty<DepartmentDashboardCriticalResponseCustomInputItem>();

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record DepartmentDashboardCriticalResponseCustomInputItem
{
    public string Name { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}
