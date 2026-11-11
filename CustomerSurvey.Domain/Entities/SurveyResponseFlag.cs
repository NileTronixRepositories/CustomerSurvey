using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Domain.Entities;

public sealed class SurveyResponseFlag : Entity<Guid>
{
    public const int MaxBriefLength = 1000;
    public const int MaxAnswerLength = 4000;

    public Guid SurveyResponseId { get; private set; }
    public SurveyResponse SurveyResponse { get; private set; } = null!;
    public SurveyResponseFlagType Type { get; private set; }
    public string? Brief { get; private set; }
    public string? Answer { get; private set; }
    public Guid CreatedByApplicationUserId { get; private set; }
    public ApplicationUser CreatedBy { get; private set; } = null!;
    public Guid? BriefUpdatedByApplicationUserId { get; private set; }
    public ApplicationUser? BriefUpdatedBy { get; private set; }
    public DateTime? BriefUpdatedOnUtc { get; private set; }
    public Guid? AnsweredByApplicationUserId { get; private set; }
    public ApplicationUser? AnsweredBy { get; private set; }
    public DateTime? AnsweredOnUtc { get; private set; }

    private SurveyResponseFlag() { }

    internal static SurveyResponseFlag CreateOnSubmit(
        Guid responseId, SurveyResponseFlagType type, string? brief, Guid creatorId)
        => Create(responseId, type, string.IsNullOrWhiteSpace(brief) ? null : brief, creatorId);

    public static SurveyResponseFlag CreateForFollowUp(
        Guid responseId, SurveyResponseFlagType type, string brief, Guid creatorId)
        => Create(responseId, type, RequiredText(brief, MaxBriefLength), creatorId);

    private static SurveyResponseFlag Create(
        Guid responseId, SurveyResponseFlagType type, string? brief, Guid creatorId)
    {
        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        if (brief?.Length > MaxBriefLength)
            throw new ArgumentException("Brief exceeds the maximum length.", nameof(brief));

        return new SurveyResponseFlag
        {
            Id = Guid.NewGuid(),
            SurveyResponseId = responseId,
            Type = type,
            Brief = brief?.Trim(),
            CreatedByApplicationUserId = creatorId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    public void UpdateBrief(string brief, Guid editorId)
    {
        Brief = RequiredText(brief, MaxBriefLength);
        BriefUpdatedByApplicationUserId = editorId;
        BriefUpdatedOnUtc = DateTime.UtcNow;
    }

    public void WriteAnswer(string answer, Guid writerId)
    {
        Answer = RequiredText(answer, MaxAnswerLength);
        AnsweredByApplicationUserId = writerId;
        AnsweredOnUtc = DateTime.UtcNow;
    }

    private static string RequiredText(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > maxLength)
            throw new ArgumentException("Text is required and must fit the maximum length.", nameof(text));

        return text.Trim();
    }
}
