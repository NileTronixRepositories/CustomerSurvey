using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

public sealed record GetAnonymousTemplateDashboardResponse
{
    public AnonymousTemplateDashboardPeriodResponse Period { get; init; } = new();

    public AnonymousTemplateDashboardSummaryResponse Summary { get; init; } = new();

    public IReadOnlyCollection<AnonymousTemplateDashboardTrendPointResponse> SatisfactionTrend { get; init; }
        = Array.Empty<AnonymousTemplateDashboardTrendPointResponse>();

    public IReadOnlyCollection<AnonymousTemplateDashboardPerformanceResponse> AnonymousTemplatePerformance { get; init; }
        = Array.Empty<AnonymousTemplateDashboardPerformanceResponse>();

    public IReadOnlyCollection<AnonymousTemplateDashboardQuestionInsightResponse> LowestRatedQuestions { get; init; }
        = Array.Empty<AnonymousTemplateDashboardQuestionInsightResponse>();

    public IReadOnlyCollection<AnonymousTemplateDashboardCustomInputSegmentResponse> CustomInputSegments { get; init; }
        = Array.Empty<AnonymousTemplateDashboardCustomInputSegmentResponse>();

    public IReadOnlyCollection<AnonymousTemplateDashboardCriticalResponseItem> CriticalResponses { get; init; }
        = Array.Empty<AnonymousTemplateDashboardCriticalResponseItem>();
}

public sealed record AnonymousTemplateDashboardPeriodResponse
{
    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public bool IsDefaultPeriod { get; init; }

    public string GroupBy { get; init; } = string.Empty;
}

public sealed record AnonymousTemplateDashboardSummaryResponse
{
    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public int TotalAnonymousTemplates { get; init; }

    public int ActiveAnonymousTemplates { get; init; }

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

public sealed record AnonymousTemplateDashboardTrendPointResponse
{
    public string Period { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }
}

public sealed record AnonymousTemplateDashboardPerformanceResponse
{
    public Guid AnonymousTemplateId { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public AnonymousTemplateScope Scope { get; init; }

    public string ScopeName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public string? LogoPath { get; init; }

    public string? PublicUrl { get; init; }

    public string? QrCode { get; init; }

    public int ResponsesCount { get; init; }

    public int ScoredResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int ComplaintsCount { get; init; }

    public string RiskLevel { get; init; } = string.Empty;
}

public sealed record AnonymousTemplateDashboardQuestionInsightResponse
{
    public Guid AnonymousTemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid AnonymousTemplateQuestionId { get; init; }

    public Guid QuestionId { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }

    public string QuestionTypeName { get; init; } = string.Empty;

    public int AnswersCount { get; init; }

    public decimal AverageValue { get; init; }

    public decimal AverageScorePercentage { get; init; }
}

public sealed record AnonymousTemplateDashboardCustomInputSegmentResponse
{
    public string CustomInputName { get; init; } = string.Empty;

    public TemplateCustomInputType Type { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public IReadOnlyCollection<AnonymousTemplateDashboardCustomInputSegmentValueResponse> Segments { get; init; }
        = Array.Empty<AnonymousTemplateDashboardCustomInputSegmentValueResponse>();
}

public sealed record AnonymousTemplateDashboardCustomInputSegmentValueResponse
{
    public string Value { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }
}

public sealed record AnonymousTemplateDashboardCriticalResponseItem
{
    public Guid AnonymousSurveyResponseId { get; init; }

    public Guid AnonymousTemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public decimal ScorePercentage { get; init; }

    public string? ComplaintText { get; init; }

    public IReadOnlyCollection<AnonymousTemplateDashboardCriticalResponseCustomInputItem> CustomInputs { get; init; }
        = Array.Empty<AnonymousTemplateDashboardCriticalResponseCustomInputItem>();
}

public sealed record AnonymousTemplateDashboardCriticalResponseCustomInputItem
{
    public string Name { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}
