using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Services.Scoring;

public sealed record ResponseFlatDto
{
    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public DateTime SubmittedOnUtc { get; init; }
}

public sealed record AnswerFlatDto
{
    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public Guid QuestionId { get; init; }

    public QuestionType QuestionType { get; init; }

    public Guid? SelectedQuestionOptionId { get; init; }

    public int? StarRatingValue { get; init; }

    public int? SmileValue { get; init; }

    public bool HasTextAnswer { get; init; }

    public bool HasVoiceAnswer { get; init; }
}

public sealed record TemplateQuestionFlatDto
{
    public Guid TemplateQuestionId { get; init; }

    public Guid TemplateId { get; init; }

    public Guid QuestionId { get; init; }

    public int Order { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }
}

public sealed record ConditionFlatDto
{
    public Guid TemplateId { get; init; }

    public Guid ParentTemplateQuestionId { get; init; }

    public Guid ChildTemplateQuestionId { get; init; }

    public QuestionConditionTriggerType TriggerType { get; init; }

    public Guid? SelectedQuestionOptionId { get; init; }

    public int? TriggerValue { get; init; }
}

public sealed record QuestionOptionFlatDto
{
    public Guid OptionId { get; init; }

    public Guid QuestionId { get; init; }

    public string TextEn { get; init; } = string.Empty;

    public string? TextAr { get; init; }

    public int Value { get; init; }

    public int Order { get; init; }
}

public sealed record CalculatedResponseScore
{
    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public int ScoredItemsCount { get; init; }

    public decimal AverageScoreValue { get; init; }

    public decimal ScorePercentage { get; init; }

    public bool HasScore => ScoredItemsCount > 0;
}

public sealed record QuestionScoreToken
{
    public Guid ResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public Guid TemplateQuestionId { get; init; }

    public Guid QuestionId { get; init; }

    public ReportTemplateKind TemplateKind { get; init; }

    public decimal ScoreValue { get; init; }

    public decimal ScorePercentage { get; init; }
}
