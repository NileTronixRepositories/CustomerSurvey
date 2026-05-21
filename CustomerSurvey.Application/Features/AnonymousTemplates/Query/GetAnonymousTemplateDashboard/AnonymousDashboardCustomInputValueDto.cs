using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed record AnonymousDashboardCustomInputValueDto
{
    public Guid AnonymousSurveyResponseId { get; init; }

    public string NameSnapshot { get; init; } = string.Empty;

    public TemplateCustomInputType TypeSnapshot { get; init; }

    public string? StringValue { get; init; }

    public int? IntegerValue { get; init; }

    public decimal ScorePercentage { get; init; }

    public int MaxScore { get; init; }
}
