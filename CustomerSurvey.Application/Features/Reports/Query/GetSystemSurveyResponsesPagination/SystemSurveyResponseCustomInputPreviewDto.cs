using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;

internal sealed record SystemSurveyResponseCustomInputPreviewDto
{
    public Guid SurveyResponseId { get; init; }

    public string NameSnapshot { get; init; } = string.Empty;

    public TemplateCustomInputType TypeSnapshot { get; init; }

    public string? StringValue { get; init; }

    public int? IntegerValue { get; init; }
}