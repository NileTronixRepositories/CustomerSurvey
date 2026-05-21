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

    public BranchTemplatesPdfExecutiveSummary ExecutiveSummary { get; init; } = new();

    public IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> Templates { get; init; }
        = Array.Empty<BranchTemplatesPdfTemplateSummary>();

    public IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> Questions { get; init; }
        = Array.Empty<BranchTemplatesPdfQuestionAnalytics>();

    public IReadOnlyCollection<BranchTemplatesPdfCustomInputSummary> CustomInputs { get; init; }
        = Array.Empty<BranchTemplatesPdfCustomInputSummary>();
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

    public string TemplateKindName => TemplateKind.ToString();

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string DisplayName(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(NameAr) ? NameAr! : NameEn;

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

    public int TotalAnswers { get; init; }

    public int SkippedCount { get; init; }

    public decimal AnswerRatePercentage { get; init; }

    public decimal? AverageValue { get; init; }

    public bool IsScoreIncluded { get; init; }

    public int ScoreIncludedAnswersCount { get; init; }

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

public sealed record BranchTemplatesPdfCustomInputSummary
{
    public Guid AnonymousTemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public string InputName { get; init; } = string.Empty;

    public string LabelEn { get; init; } = string.Empty;

    public string? LabelAr { get; init; }

    public string Type { get; init; } = string.Empty;

    public bool IsRequired { get; init; }

    public int FilledCount { get; init; }

    public int EmptyCount { get; init; }

    public decimal CompletionRatePercentage { get; init; }

    public string DisplayTemplateName(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(TemplateNameAr)
            ? TemplateNameAr!
            : TemplateNameEn;

    public string DisplayLabel(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(LabelAr)
            ? LabelAr!
            : LabelEn;
}