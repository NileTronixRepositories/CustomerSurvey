using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed record BranchSurveyAnswerDetailsDto
{
    public Guid SurveyResponseId { get; init; }

    public Guid QuestionId { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }

    public Guid? SelectedQuestionOptionId { get; init; }

    public int? StarRatingValue { get; init; }

    public int? SmileValue { get; init; }

    public string? TextAnswer { get; init; }

    public string? VoiceFileName { get; init; }
}