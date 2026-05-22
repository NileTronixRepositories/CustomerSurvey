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

    public string Status { get; init; } = string.Empty;

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
