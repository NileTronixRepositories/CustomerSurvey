using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

public sealed record SurveyDashboardResponse
{
    public SurveyDashboardPeriodResponse Period { get; init; } = new();

    public SurveyDashboardScopeResponse Scope { get; init; } = new();

    public SurveyDashboardFiltersResponse Filters { get; init; } = new();

    public SurveyDashboardSummaryResponse Summary { get; init; } = new();

    public SurveyDashboardSourceBreakdownResponse SourceBreakdown { get; init; } = new();

    public IReadOnlyCollection<SurveyDashboardBranchSummaryResponse> BranchesSummary { get; init; }
        = Array.Empty<SurveyDashboardBranchSummaryResponse>();

    public IReadOnlyCollection<SurveyDashboardTrendItemResponse> SatisfactionTrend { get; init; }
        = Array.Empty<SurveyDashboardTrendItemResponse>();

    public IReadOnlyCollection<SurveyDashboardTemplatePerformanceItemResponse> TemplatePerformance { get; init; }
        = Array.Empty<SurveyDashboardTemplatePerformanceItemResponse>();

    public IReadOnlyCollection<SurveyDashboardLowestRatedQuestionItemResponse> LowestRatedQuestions { get; init; }
        = Array.Empty<SurveyDashboardLowestRatedQuestionItemResponse>();

    public IReadOnlyCollection<SurveyDashboardCustomInputSegmentResponse> CustomInputSegments { get; init; }
        = Array.Empty<SurveyDashboardCustomInputSegmentResponse>();

    public IReadOnlyCollection<SurveyDashboardCriticalResponseItemResponse> CriticalResponses { get; init; }
        = Array.Empty<SurveyDashboardCriticalResponseItemResponse>();
}

public sealed record SurveyDashboardPeriodResponse
{
    public DateTime From { get; init; }

    public DateTime To { get; init; }

    public bool IsDefaultPeriod { get; init; }

    public DashboardGroupBy GroupBy { get; init; }
}

public sealed record SurveyDashboardScopeResponse
{
    public string ActorScope { get; init; } = string.Empty;

    public string DataScope { get; init; } = string.Empty;

    public Guid? BranchId { get; init; }

    public string? BranchNameEn { get; init; }

    public string? BranchNameAr { get; init; }
}

public sealed record SurveyDashboardFiltersResponse
{
    public SurveyDashboardSource Source { get; init; }

    public Guid? BranchId { get; init; }

    public Guid? TemplateId { get; init; }

    public Guid? AnonymousTemplateId { get; init; }

    public int TopQuestionsCount { get; init; }

    public int CriticalResponsesCount { get; init; }

    public decimal CriticalScoreThreshold { get; init; }
}

public sealed record SurveyDashboardSummaryResponse
{
    public int TotalResponses { get; init; }

    public int ScoredResponses { get; init; }

    public int UnscoredResponses { get; init; }

    public int InternalResponses { get; init; }

    public int AnonymousResponses { get; init; }

    public int InternalScoredResponses { get; init; }

    public int AnonymousScoredResponses { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int SatisfiedResponses { get; init; }

    public int NeutralResponses { get; init; }

    public int UnhappyResponses { get; init; }

    public int ComplaintsCount { get; init; }

    public int VoiceAnswersCount { get; init; }

    public int ActiveInternalTemplatesCount { get; init; }

    public int ActiveAnonymousTemplatesCount { get; init; }

    public int TemplatesWithResponsesCount { get; init; }

    public int BranchesCount { get; init; }

    public int BranchesWithResponsesCount { get; init; }
}

public sealed record SurveyDashboardSourceBreakdownResponse
{
    public SurveyDashboardSourceSummaryResponse Internal { get; init; } = new();

    public SurveyDashboardSourceSummaryResponse Anonymous { get; init; } = new();
}

public sealed record SurveyDashboardSourceSummaryResponse
{
    public int ResponsesCount { get; init; }

    public int ScoredResponsesCount { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int SatisfiedResponses { get; init; }

    public int NeutralResponses { get; init; }

    public int UnhappyResponses { get; init; }

    public int ComplaintsCount { get; init; }

    public int VoiceAnswersCount { get; init; }
}

public sealed record SurveyDashboardBranchSummaryResponse
{
    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public int TotalResponses { get; init; }

    public int InternalResponses { get; init; }

    public int AnonymousResponses { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public int ComplaintsCount { get; init; }

    public int VoiceAnswersCount { get; init; }

    public SurveyDashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SurveyDashboardTrendItemResponse
{
    public string Period { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public int InternalResponses { get; init; }

    public int AnonymousResponses { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public decimal? InternalAverageScorePercentage { get; init; }

    public decimal? AnonymousAverageScorePercentage { get; init; }
}

public sealed record SurveyDashboardTemplatePerformanceItemResponse
{
    public SurveyDashboardSource Source { get; init; }

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

    public int VoiceAnswersCount { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public SurveyDashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SurveyDashboardLowestRatedQuestionItemResponse
{
    public SurveyDashboardSource Source { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public Guid QuestionId { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }

    public string QuestionTypeName { get; init; } = string.Empty;

    public int AnswersCount { get; init; }

    public decimal AverageValue { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public SurveyDashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SurveyDashboardCustomInputSegmentResponse
{
    public SurveyDashboardSource Source { get; init; }

    public string CustomInputName { get; init; } = string.Empty;

    public string? LabelEn { get; init; }

    public string? LabelAr { get; init; }

    public TemplateCustomInputType Type { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public IReadOnlyCollection<SurveyDashboardCustomInputSegmentItemResponse> Segments { get; init; }
        = Array.Empty<SurveyDashboardCustomInputSegmentItemResponse>();
}

public sealed record SurveyDashboardCustomInputSegmentItemResponse
{
    public string Value { get; init; } = string.Empty;

    public int ResponsesCount { get; init; }

    public int InternalResponses { get; init; }

    public int AnonymousResponses { get; init; }

    public decimal AverageScorePercentage { get; init; }

    public SurveyDashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SurveyDashboardCriticalResponseItemResponse
{
    public SurveyDashboardSource Source { get; init; }

    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public decimal ScorePercentage { get; init; }

    public string? ComplaintText { get; init; }

    public bool HasComplaint { get; init; }

    public bool HasVoice { get; init; }

    public Guid? OperatorId { get; init; }

    public string? OperatorNameEn { get; init; }

    public string? OperatorNameAr { get; init; }

    public IReadOnlyCollection<SurveyDashboardCustomInputPreviewResponse> CustomInputsPreview { get; init; }
        = Array.Empty<SurveyDashboardCustomInputPreviewResponse>();

    public SurveyDashboardDetailsNavigationResponse? DetailsNavigation { get; init; }
}

public sealed record SurveyDashboardCustomInputPreviewResponse
{
    public string Name { get; init; } = string.Empty;

    public string? LabelEn { get; init; }

    public string? LabelAr { get; init; }

    public string Value { get; init; } = string.Empty;
}

public sealed record SurveyDashboardDetailsNavigationResponse
{
    public string RouteType { get; init; } = string.Empty;

    public string Method { get; init; } = "GET";

    public string Path { get; init; } = string.Empty;
}

internal sealed record SurveyDashboardBranchRow
{
    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public bool IsActive { get; init; }
}

internal sealed record SurveyDashboardTemplateRow
{
    public SurveyDashboardSource Source { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public bool IsActive { get; init; }
}

internal sealed record SurveyDashboardResponseRow
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

    public int MaxScore { get; init; }

    public decimal ScorePercentage { get; init; }

    public bool HasComplaint { get; init; }

    public bool HasVoice { get; init; }
}

internal sealed record SurveyDashboardAnswerRow
{
    public SurveyDashboardSource Source { get; init; }

    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public Guid QuestionId { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }

    public int? StarRatingValue { get; init; }

    public int? SmileValue { get; init; }

    public int? SelectedQuestionOptionValue { get; init; }

    public string? TextAnswer { get; init; }

    public string? VoiceFileName { get; init; }
}

internal sealed record SurveyDashboardCustomInputValueRow
{
    public SurveyDashboardSource Source { get; init; }

    public Guid ResponseId { get; init; }

    public Guid BranchId { get; init; }

    public string NameSnapshot { get; init; } = string.Empty;

    public string? LabelEn { get; init; }

    public string? LabelAr { get; init; }

    public TemplateCustomInputType TypeSnapshot { get; init; }

    public string? StringValue { get; init; }

    public int? IntegerValue { get; init; }

    public decimal ScorePercentage { get; init; }

    public int MaxScore { get; init; }
}
