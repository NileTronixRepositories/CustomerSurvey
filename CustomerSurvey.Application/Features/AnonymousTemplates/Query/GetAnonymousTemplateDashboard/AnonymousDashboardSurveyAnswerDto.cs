using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed record AnonymousDashboardSurveyAnswerDto
{
    public Guid AnonymousSurveyResponseId { get; init; }

    public Guid AnonymousTemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid AnonymousTemplateQuestionId { get; init; }

    public Guid QuestionId { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }

    public int? StarRatingValue { get; init; }

    public int? SmileValue { get; init; }

    public string? TextAnswer { get; init; }

    public string? VoiceFileName { get; init; }
}
