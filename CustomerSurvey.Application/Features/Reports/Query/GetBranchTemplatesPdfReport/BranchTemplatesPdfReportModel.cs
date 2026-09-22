using CustomerSurvey.Domain.Enums;
using System.Text.Json.Serialization;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

public sealed record BranchTemplatesPdfReportModel
{
    public string Language { get; init; } = "en";

    public bool IsArabic =>
        string.Equals(Language, "ar", StringComparison.OrdinalIgnoreCase);

    public string Direction => IsArabic ? "rtl" : "ltr";

    public string BranchName { get; init; } = string.Empty;

    public string GeneratedBy { get; init; } = string.Empty;

    public DateTime GeneratedAtUtc { get; init; }

    public DateOnly FromDate { get; init; }

    public DateOnly ToDate { get; init; }

    public Guid? SelectedTemplateId { get; init; }

    public string SelectedTemplateName { get; init; } = string.Empty;

    public ReportTemplateKind? SelectedTemplateKind { get; init; }

    public ScoreCalculationMode ScoreCalculationMode { get; init; }
        = ScoreCalculationMode.RootQuestions;

    public int TopWorstQuestionsCount { get; init; } = 5;

    public decimal WorstQuestionsMaxScorePercentage { get; init; }
        = BranchTemplatesReportQuestionRankThresholds.DefaultWorstQuestionsMaxScorePercentage;

    public decimal BestQuestionsMinScorePercentage { get; init; }
        = BranchTemplatesReportQuestionRankThresholds.DefaultBestQuestionsMinScorePercentage;

    public BranchTemplatesPdfExecutiveSummary ExecutiveSummary { get; init; } = new();

    public IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> Templates { get; init; }
        = Array.Empty<BranchTemplatesPdfTemplateSummary>();

    public IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> Questions { get; init; }
        = Array.Empty<BranchTemplatesPdfQuestionAnalytics>();

    public IReadOnlyCollection<BranchTemplatesPdfQuestionRankItem> WorstQuestions { get; init; }
        = Array.Empty<BranchTemplatesPdfQuestionRankItem>();

    public IReadOnlyCollection<BranchTemplatesPdfQuestionRankItem> BestQuestions { get; init; }
        = Array.Empty<BranchTemplatesPdfQuestionRankItem>();

    public IReadOnlyCollection<BranchTemplatesPdfTemplateDetail> TemplateDetails { get; init; }
        = Array.Empty<BranchTemplatesPdfTemplateDetail>();

    [JsonIgnore]
    public IReadOnlyCollection<BranchTemplatesReportCustomInputDefinition> CustomInputDefinitions { get; init; }
        = Array.Empty<BranchTemplatesReportCustomInputDefinition>();

    [JsonIgnore]
    public IReadOnlyCollection<BranchTemplatesReportResponse> Responses { get; init; }
        = Array.Empty<BranchTemplatesReportResponse>();

    [JsonIgnore]
    public BranchTemplatesReportGraphics Graphics { get; init; } = new();

    public IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> TemplatesWithoutResponses =>
        Templates
            .Where(x => x.TotalResponses == 0)
            .ToArray();
}

public sealed record BranchTemplatesPdfExecutiveSummary
{
    public int TotalNormalTemplates { get; init; }

    public int TotalAnonymousTemplates { get; init; }

    public int TotalTemplates => TotalNormalTemplates + TotalAnonymousTemplates;

    public int TotalResponses { get; init; }

    public int TotalNormalResponses { get; init; }

    public int TotalAnonymousResponses { get; init; }

    public int TotalAnswers { get; init; }

    public int TotalScoredAnswers { get; init; }

    public int TotalNonScoredAnswers { get; init; }

    public decimal? AverageScoreValue { get; init; }

    public decimal? AverageScorePercentage { get; init; }

    public string HighestRatedTemplateName { get; init; } = "-";

    public string LowestRatedTemplateName { get; init; } = "-";

    public string MostAnsweredTemplateName { get; init; } = "-";

    public int TemplatesWithoutResponses { get; init; }
}

public sealed record BranchTemplatesPdfTemplateSummary
{
    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string DisplayName(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(NameAr) ? NameAr! : NameEn;

    public string DisplayKind(bool isArabic)
        => TemplateKind == ReportTemplateKind.Normal
            ? isArabic ? "مصرح" : "Authorized"
            : isArabic ? "مجهول" : "Anonymous";

    public DateTime? ActiveFrom { get; init; }

    public DateTime? ExpireTo { get; init; }

    public int TotalQuestions { get; init; }

    public int RootQuestions { get; init; }

    public int ConditionalQuestions { get; init; }

    public int TotalResponses { get; init; }

    public int TotalAnswers { get; init; }

    public int TotalScoredAnswers { get; init; }

    public decimal? AverageScoreValue { get; init; }

    public decimal? AverageScorePercentage { get; init; }
}

public sealed record BranchTemplatesPdfQuestionAnalytics
{
    private const decimal MaxScoreValue = 5m;

    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid TemplateQuestionId { get; init; }

    public Guid QuestionId { get; init; }

    [JsonIgnore]
    public int QuestionOrder { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public string QuestionType { get; init; } = string.Empty;

    public bool IsRootQuestion { get; init; }

    public string? ParentTriggerTextEn { get; init; }

    public string? ParentTriggerTextAr { get; init; }

    public int TotalAnswers { get; init; }

    public int SkippedCount { get; init; }

    public decimal? AverageValue { get; init; }

    public decimal? ScoreAverageValue { get; init; }

    public decimal? ScoreAveragePercentage =>
        ScoreAverageValue.HasValue
            ? Math.Round(ScoreAverageValue.Value * 100m / MaxScoreValue, 2)
            : null;

    public bool IsScoreIncluded { get; init; }

    public IReadOnlyCollection<BranchTemplatesPdfOptionAnalytics> Options { get; init; }
        = Array.Empty<BranchTemplatesPdfOptionAnalytics>();

    public string DisplayTemplateName(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(TemplateNameAr)
            ? TemplateNameAr!
            : TemplateNameEn;

    public string DisplayQuestionText(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(QuestionTextAr)
            ? QuestionTextAr!
            : QuestionTextEn;

    public string DisplayParentTrigger(bool isArabic)
    {
        var value = isArabic && !string.IsNullOrWhiteSpace(ParentTriggerTextAr)
            ? ParentTriggerTextAr
            : ParentTriggerTextEn;

        return string.IsNullOrWhiteSpace(value) ? "-" : value!;
    }

    public string DisplayLevel(bool isArabic)
        => IsRootQuestion
            ? isArabic ? "رئيسي" : "Root"
            : isArabic ? "شرطي" : "Conditional";

    public string DisplayKind(bool isArabic)
        => TemplateKind == ReportTemplateKind.Normal
            ? isArabic ? "مصرح" : "Authorized"
            : isArabic ? "مجهول" : "Anonymous";
}

public sealed record BranchTemplatesPdfOptionAnalytics
{
    public string LabelEn { get; init; } = string.Empty;

    public string? LabelAr { get; init; }

    public int? Value { get; init; }

    public int Count { get; init; }

    public decimal Percentage { get; init; }

    public string DisplayLabel(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(LabelAr) ? LabelAr! : LabelEn;
}

public sealed record BranchTemplatesPdfQuestionRankItem
{
    public int Rank { get; init; }

    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid TemplateQuestionId { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public bool IsRootQuestion { get; init; }

    public string QuestionType { get; init; } = string.Empty;

    public int TotalAnswers { get; init; }

    public decimal AverageScoreValue { get; init; }

    public decimal SatisfactionPercentage { get; init; }

    public string DisplayTemplateName(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(TemplateNameAr)
            ? TemplateNameAr!
            : TemplateNameEn;

    public string DisplayQuestionText(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(QuestionTextAr)
            ? QuestionTextAr!
            : QuestionTextEn;

    public string DisplayLevel(bool isArabic)
        => IsRootQuestion
            ? isArabic ? "رئيسي" : "Root"
            : isArabic ? "شرطي" : "Conditional";

    public string DisplayKind(bool isArabic)
        => TemplateKind == ReportTemplateKind.Normal
            ? isArabic ? "مصرح" : "Authorized"
            : isArabic ? "مجهول" : "Anonymous";
}

public sealed record BranchTemplatesPdfTemplateDetail
{
    public BranchTemplatesPdfTemplateSummary Summary { get; init; } = new();

    public IReadOnlyCollection<BranchTemplatesPdfFlowLine> FlowLines { get; init; }
        = Array.Empty<BranchTemplatesPdfFlowLine>();

    public IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> Questions { get; init; }
        = Array.Empty<BranchTemplatesPdfQuestionAnalytics>();
}

public sealed record BranchTemplatesPdfFlowLine
{
    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public string Number { get; init; } = string.Empty;

    public int Depth { get; init; }

    public BranchTemplatesPdfFlowLineKind LineKind { get; init; }

    public string TextEn { get; init; } = string.Empty;

    public string? TextAr { get; init; }

    public string? QuestionType { get; init; }

    public int? Value { get; init; }

    public bool IsQuestion =>
        LineKind is BranchTemplatesPdfFlowLineKind.RootQuestion
            or BranchTemplatesPdfFlowLineKind.ConditionalQuestion;

    public bool IsRootQuestion =>
        LineKind == BranchTemplatesPdfFlowLineKind.RootQuestion;

    public string DisplayText(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(TextAr) ? TextAr! : TextEn;
}

public enum BranchTemplatesPdfFlowLineKind
{
    RootQuestion = 1,

    ConditionalQuestion = 2,

    Trigger = 3
}

public sealed record BranchTemplatesReportGraphics
{
    public decimal? OverallSatisfactionPercentage { get; init; }

    public decimal? AverageScoreValue { get; init; }

    public int TotalResponses { get; init; }

    public int ScoredResponses { get; init; }

    public int NotScoredResponses { get; init; }

    public int ExcellentResponses { get; init; }

    public int GoodResponses { get; init; }

    public int AverageResponses { get; init; }

    public int PoorResponses { get; init; }

    public int CriticalResponses { get; init; }

    public int RootQuestions { get; init; }

    public int ConditionalQuestions { get; init; }

    public int IncludedAnswers { get; init; }

    public int NonScoredAnswers { get; init; }

    public int ScoreDistributionTotal =>
        ExcellentResponses + GoodResponses + AverageResponses + PoorResponses + CriticalResponses;
}

public sealed record BranchTemplatesReportCustomInputDefinition
{
    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public Guid CustomInputId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? LabelEn { get; init; }

    public string? LabelAr { get; init; }

    public TemplateCustomInputType Type { get; init; }

    public int Order { get; init; }

    public string DisplayName(bool isArabic)
    {
        if (isArabic && !string.IsNullOrWhiteSpace(LabelAr))
        {
            return LabelAr!;
        }

        if (!string.IsNullOrWhiteSpace(LabelEn))
        {
            return LabelEn!;
        }

        return Name;
    }
}

public sealed record BranchTemplatesReportResponse
{
    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public Guid? OperatorId { get; init; }

    public string? OperatorNameEn { get; init; }

    public string? OperatorNameAr { get; init; }

    public bool IsScored { get; init; }

    public int ScoredItemsCount { get; init; }

    public decimal? AverageScoreValue { get; init; }

    public decimal MaxScore { get; init; } = 5m;

    public decimal? ScorePercentage { get; init; }

    public IReadOnlyCollection<BranchTemplatesReportCustomInputValue> CustomInputs { get; init; }
        = Array.Empty<BranchTemplatesReportCustomInputValue>();

    public IReadOnlyCollection<BranchTemplatesReportAnswer> Answers { get; init; }
        = Array.Empty<BranchTemplatesReportAnswer>();

    public string DisplayTemplateName(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(TemplateNameAr)
            ? TemplateNameAr!
            : TemplateNameEn;

    public string DisplayOperatorName(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(OperatorNameAr)
            ? OperatorNameAr!
            : OperatorNameEn ?? string.Empty;
}

public sealed record BranchTemplatesReportCustomInputValue
{
    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public Guid CustomInputId { get; init; }

    public string Name { get; init; } = string.Empty;

    public TemplateCustomInputType Type { get; init; }

    public int? Order { get; init; }

    public string? StringValue { get; init; }

    public int? IntegerValue { get; init; }

    public string DisplayValue => Type switch
    {
        TemplateCustomInputType.String => StringValue ?? string.Empty,
        TemplateCustomInputType.Integer => IntegerValue?.ToString() ?? string.Empty,
        _ => string.Empty
    };
}

public sealed record BranchTemplatesReportAnswer
{
    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public Guid? TemplateQuestionId { get; init; }

    public Guid QuestionId { get; init; }

    public int? QuestionOrder { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }

    public bool IsRootQuestion { get; init; }

    public string? ParentTriggerTextEn { get; init; }

    public string? ParentTriggerTextAr { get; init; }

    public Guid? SelectedQuestionOptionId { get; init; }

    public string? SelectedOptionTextEn { get; init; }

    public string? SelectedOptionTextAr { get; init; }

    public int? SelectedOptionValue { get; init; }

    public int? StarRatingValue { get; init; }

    public int? SmileValue { get; init; }

    public string? TextAnswer { get; init; }

    public string? VoiceFileName { get; init; }

    public string? VoiceFilePath { get; init; }

    public string? ImageFileName { get; init; }

    public string? ImageFilePath { get; init; }

    public string DisplayValue { get; init; } = string.Empty;

    public bool IsScorable { get; init; }

    public decimal? ScoreValue { get; init; }

    public bool IncludedInScore { get; init; }

    public string ScoreInclusionReason { get; init; } = string.Empty;

    public string DisplayQuestionText(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(QuestionTextAr)
            ? QuestionTextAr!
            : QuestionTextEn;

    public string DisplaySelectedOption(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(SelectedOptionTextAr)
            ? SelectedOptionTextAr!
            : SelectedOptionTextEn ?? string.Empty;

    public string DisplayParentTrigger(bool isArabic)
    {
        var value = isArabic && !string.IsNullOrWhiteSpace(ParentTriggerTextAr)
            ? ParentTriggerTextAr
            : ParentTriggerTextEn;

        return string.IsNullOrWhiteSpace(value) ? string.Empty : value!;
    }
}
