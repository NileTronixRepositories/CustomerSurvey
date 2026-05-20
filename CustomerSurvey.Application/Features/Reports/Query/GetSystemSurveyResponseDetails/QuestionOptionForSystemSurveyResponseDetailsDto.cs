namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

internal sealed record QuestionOptionForSystemSurveyResponseDetailsDto
{
    public Guid OptionId { get; init; }

    public string TextEn { get; init; } = string.Empty;

    public string? TextAr { get; init; }

    public int Value { get; init; }
}