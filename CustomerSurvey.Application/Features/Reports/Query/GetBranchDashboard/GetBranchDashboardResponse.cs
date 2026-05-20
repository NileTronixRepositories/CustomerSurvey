using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

public sealed record GetBranchDashboardResponse
{
    public BranchDashboardPeriodResponse Period { get; init; } = new();

    public BranchDashboardSummaryResponse Summary { get; init; } = new();

    public IReadOnlyCollection<BranchDashboardTrendPointResponse> SatisfactionTrend { get; init; }
        = Array.Empty<BranchDashboardTrendPointResponse>();

    public IReadOnlyCollection<BranchDashboardTemplatePerformanceResponse> TemplatePerformance { get; init; }
        = Array.Empty<BranchDashboardTemplatePerformanceResponse>();

    public IReadOnlyCollection<BranchDashboardQuestionInsightResponse> LowestRatedQuestions { get; init; }
        = Array.Empty<BranchDashboardQuestionInsightResponse>();

    public IReadOnlyCollection<BranchDashboardCustomInputSegmentResponse> CustomInputSegments { get; init; }
        = Array.Empty<BranchDashboardCustomInputSegmentResponse>();

    public IReadOnlyCollection<BranchDashboardCriticalResponseItem> CriticalResponses { get; init; }
        = Array.Empty<BranchDashboardCriticalResponseItem>();
}

public sealed record BranchDashboardPeriodResponse
{
    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public bool IsDefaultPeriod { get; init; }

    public string GroupBy { get; init; } = string.Empty;
}

public sealed record BranchDashboardSummaryResponse
{
    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public int TotalResponses { get; init; }

    public int ScoredResponses { get; init; }

    public int UnscoredResponses { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int SatisfiedResponses { get; init; }

    public int NeutralResponses { get; init; }

    public int UnhappyResponses { get; init; }

    public int ActiveTemplatesCount { get; init; }

    public int TemplatesWithResponsesCount { get; init; }

    public int ComplaintsCount { get; init; }

    public int VoiceAnswersCount { get; init; }
}

public sealed record BranchDashboardTrendPointResponse
{
    public string Period { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }
}

public sealed record BranchDashboardTemplatePerformanceResponse
{
    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public int ResponsesCount { get; init; }

    public int ScoredResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int ComplaintsCount { get; init; }

    public string RiskLevel { get; init; } = string.Empty;
}

public sealed record BranchDashboardQuestionInsightResponse
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
}

public sealed record BranchDashboardCustomInputSegmentResponse
{
    public string CustomInputName { get; init; } = string.Empty;

    public TemplateCustomInputType Type { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public IReadOnlyCollection<BranchDashboardCustomInputSegmentValueResponse> Segments { get; init; }
        = Array.Empty<BranchDashboardCustomInputSegmentValueResponse>();
}

public sealed record BranchDashboardCustomInputSegmentValueResponse
{
    public string Value { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }
}

public sealed record BranchDashboardCriticalResponseItem
{
    public Guid SurveyResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public decimal ScorePercentage { get; init; }

    public string? ComplaintText { get; init; }

    public IReadOnlyCollection<BranchDashboardCriticalResponseCustomInputItem> CustomInputs { get; init; }
        = Array.Empty<BranchDashboardCriticalResponseCustomInputItem>();
}

public sealed record BranchDashboardCriticalResponseCustomInputItem
{
    public string Name { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}