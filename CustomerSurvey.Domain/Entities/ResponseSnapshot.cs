using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using System.Text.Json;

namespace CustomerSurvey.Domain.Entities;

public enum SnapshotProvenance { CapturedAtSubmission = 1, LegacyReconstructed = 2, LegacyIncomplete = 3 }

// The payload is private to persistence. Read APIs expose a typed, localized interpretation.
public sealed class ResponseSnapshot : Entity<Guid>
{
    public Guid? SurveyResponseId { get; private set; }
    public Guid? AnonymousSurveyResponseId { get; private set; }
    public Guid ResponseId { get; private set; }
    public Guid TemplateId { get; private set; }
    public int TemplateKind { get; private set; }
    public int SchemaVersion { get; private set; } = 1;
    public SnapshotProvenance Provenance { get; private set; }
    public DateTime CapturedOnUtc { get; private set; }
    public string Payload { get; private set; } = string.Empty;

    public string RootQuestionIds { get; private set; } = "|";
    public string LowestQuestionIds { get; private set; } = "|";
    public string AllQuestionIds { get; private set; } = "|";
    public decimal? RootScorePercentage { get; private set; }
    public int RootScoredItemsCount { get; private set; }
    public decimal? LowestScorePercentage { get; private set; }
    public int LowestScoredItemsCount { get; private set; }
    public decimal? AllScorePercentage { get; private set; }
    public int AllScoredItemsCount { get; private set; }

    public static ResponseSnapshot Create(ResponseSnapshotDocument document)
    {
        var root = SnapshotScoreSelection.Questions(document, 1);
        var lowest = SnapshotScoreSelection.Questions(document, 2);
        var all = SnapshotScoreSelection.Questions(document, 3);
        decimal? Percentage(IReadOnlyList<SnapshotQuestion> q) => q.Count == 0 ? null :
            Math.Round(q.Average(x => (decimal)SnapshotScoreSelection.Value(x)!.Value) * 20m, 2, MidpointRounding.AwayFromZero);
        return new() { Id = Guid.NewGuid(), ResponseId = document.ResponseId, TemplateId = document.TemplateId,
            TemplateKind = document.TemplateKind, SurveyResponseId = document.TemplateKind == 1 ? document.ResponseId : null,
            AnonymousSurveyResponseId = document.TemplateKind == 2 ? document.ResponseId : null,
            Provenance = document.Provenance, CapturedOnUtc = DateTime.UtcNow,
            RootQuestionIds = "|" + string.Join("|", root.Select(x => x.QuestionId)) + "|",
            LowestQuestionIds = "|" + string.Join("|", lowest.Select(x => x.QuestionId)) + "|",
            AllQuestionIds = "|" + string.Join("|", all.Select(x => x.QuestionId)) + "|",
            RootScorePercentage = Percentage(root), RootScoredItemsCount = root.Count,
            LowestScorePercentage = Percentage(lowest), LowestScoredItemsCount = lowest.Count,
            AllScorePercentage = Percentage(all), AllScoredItemsCount = all.Count,
            Payload = JsonSerializer.Serialize(document) };
    }

    public ResponseSnapshotDocument Read() => JsonSerializer.Deserialize<ResponseSnapshotDocument>(Payload)!;
}

public sealed record ResponseSnapshotDocument
{
    public int SchemaVersion { get; init; } = 1;
    public Guid ResponseId { get; init; }
    public Guid TemplateId { get; init; }
    public int TemplateKind { get; init; }
    public string TemplateNameEn { get; init; } = string.Empty;
    public string? TemplateNameAr { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? OperatorId { get; init; }
    public DateTime SubmittedOnUtc { get; init; }
    public int ActualScore { get; init; }
    public int MaxScore { get; init; }
    public decimal ScorePercentage { get; init; }
    public SnapshotProvenance Provenance { get; init; }
    public bool IsHistoricallyVerified => Provenance == SnapshotProvenance.CapturedAtSubmission;
    public IReadOnlyList<SnapshotQuestion> Questions { get; init; } = Array.Empty<SnapshotQuestion>();
    public IReadOnlyList<SnapshotCondition> Conditions { get; init; } = Array.Empty<SnapshotCondition>();
    // Labels/types/values already live in the immutable response custom input value rows.
    public IReadOnlyList<SnapshotCustomInputDefinition> CustomInputDefinitions { get; init; } = Array.Empty<SnapshotCustomInputDefinition>();
}

public sealed record SnapshotQuestion
{
    public Guid TemplateQuestionId { get; init; }
    public Guid QuestionId { get; init; }
    public QuestionType Type { get; init; }
    public string TextEn { get; init; } = string.Empty;
    public string? TextAr { get; init; }
    public int Order { get; init; }
    public Guid GroupId { get; init; }
    public string GroupNameEn { get; init; } = string.Empty;
    public string? GroupNameAr { get; init; }
    public bool IsRoot { get; init; }
    public bool IsVisible { get; init; } = true;
    public bool IsRequired { get; init; } = true;
    public bool HasAnswer { get; init; }
    public Guid? SelectedQuestionOptionId { get; init; }
    public int? SelectedOptionValue { get; init; }
    public string? SelectedOptionTextEn { get; init; }
    public string? SelectedOptionTextAr { get; init; }
    public int? StarRatingValue { get; init; }
    public int? SmileValue { get; init; }
    public string? TextAnswer { get; init; }
    public string? VoiceFileName { get; init; }
    public string? ImageFileName { get; init; }
    public IReadOnlyList<SnapshotOption> Options { get; init; } = Array.Empty<SnapshotOption>();
}

public sealed record SnapshotOption(Guid OptionId, string TextEn, string? TextAr, int Value, int Order, bool IsActive);
public sealed record SnapshotCondition(Guid ParentTemplateQuestionId, Guid ChildTemplateQuestionId,
    QuestionConditionTriggerType TriggerType, Guid? SelectedQuestionOptionId, int? TriggerValue, int Order);
public sealed record SnapshotCustomInputDefinition(Guid CustomInputId, string? LabelEn, string? LabelAr,
    TemplateCustomInputType Type, bool IsRequired, int? MinLength, int? MaxLength, int? MinValue, int? MaxValue,
    string? StartWith, int Order);
