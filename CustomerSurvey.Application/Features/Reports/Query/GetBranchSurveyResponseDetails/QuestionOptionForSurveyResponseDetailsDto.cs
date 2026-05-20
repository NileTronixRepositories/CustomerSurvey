namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed record QuestionOptionForSurveyResponseDetailsDto
{
    public Guid OptionId { get; init; }

    public string TextEn { get; init; } = string.Empty;

    public string? TextAr { get; init; }

    public int Value { get; init; }
}