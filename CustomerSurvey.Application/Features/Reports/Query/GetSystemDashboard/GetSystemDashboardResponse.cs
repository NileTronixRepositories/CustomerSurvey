namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

public sealed record GetSystemDashboardResponse
{
    public SystemDashboardPeriodResponse Period { get; init; } = new();

    public SystemDashboardSummaryResponse Summary { get; init; } = new();

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardChartsResponse Charts { get; init; } = new();

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardSummaryActionsResponse SummaryActions { get; init; } = new();

    public IReadOnlyCollection<SystemDashboardTrendPointResponse> SatisfactionTrend { get; init; }
        = Array.Empty<SystemDashboardTrendPointResponse>();

    public IReadOnlyCollection<SystemDashboardBranchPerformanceResponse> BranchPerformance { get; init; }
        = Array.Empty<SystemDashboardBranchPerformanceResponse>();

    public IReadOnlyCollection<SystemDashboardDepartmentActivityResponse> DepartmentActivity { get; init; }
        = Array.Empty<SystemDashboardDepartmentActivityResponse>();

    public IReadOnlyCollection<SystemDashboardTopTemplateResponse> TopTemplates { get; init; }
        = Array.Empty<SystemDashboardTopTemplateResponse>();

    public IReadOnlyCollection<SystemDashboardCriticalResponseItem> CriticalResponses { get; init; }
        = Array.Empty<SystemDashboardCriticalResponseItem>();
}

public sealed record SystemDashboardPeriodResponse
{
    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public bool IsDefaultPeriod { get; init; }

    public string GroupBy { get; init; } = string.Empty;
}

public sealed record SystemDashboardSummaryResponse
{
    public int TotalBranches { get; init; }

    public int ActiveBranches { get; init; }

    public int InactiveBranches { get; init; }

    public int TotalDepartments { get; init; }

    public int ActiveDepartments { get; init; }

    public int TotalOperators { get; init; }

    public int TotalTemplates { get; init; }

    public int ActiveTemplates { get; init; }

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

public sealed record SystemDashboardTrendPointResponse
{
    public string Period { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SystemDashboardBranchPerformanceResponse
{
    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public string BranchCode { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public int ScoredResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int ComplaintsCount { get; init; }

    public int VoiceAnswersCount { get; init; }

    public int ActiveTemplatesCount { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SystemDashboardDepartmentActivityResponse
{
    public Guid DepartmentId { get; init; }

    public string DepartmentNameEn { get; init; } = string.Empty;

    public string? DepartmentNameAr { get; init; }

    public int OperatorsCount { get; init; }

    public int ResponsesCount { get; init; }

    public DateTime? LastResponseOnUtc { get; init; }

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SystemDashboardTopTemplateResponse
{
    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public int ResponsesCount { get; init; }

    public int ScoredResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int ComplaintsCount { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SystemDashboardCriticalResponseItem
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

    public IReadOnlyCollection<SystemDashboardCriticalResponseCustomInputItem> CustomInputs { get; init; }
        = Array.Empty<SystemDashboardCriticalResponseCustomInputItem>();

    public CustomerSurvey.Application.Features.Reports.Shared.DashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SystemDashboardCriticalResponseCustomInputItem
{
    public string Name { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}
