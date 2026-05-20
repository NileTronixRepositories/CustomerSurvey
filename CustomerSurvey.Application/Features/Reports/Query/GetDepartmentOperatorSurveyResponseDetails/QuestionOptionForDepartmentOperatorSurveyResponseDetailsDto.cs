namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed record QuestionOptionForDepartmentOperatorSurveyResponseDetailsDto
{
    public Guid OptionId { get; init; }

    public string TextEn { get; init; } = string.Empty;

    public string? TextAr { get; init; }

    public int Value { get; init; }
}
