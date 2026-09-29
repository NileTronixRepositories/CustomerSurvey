using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed record SystemDashboardCustomInputPreviewDto
{
    public Guid SurveyResponseId { get; init; }

    public Guid CustomInputId { get; init; }

    public string? LabelEnSnapshot { get; init; }

    public string? LabelArSnapshot { get; init; }

    public TemplateCustomInputType TypeSnapshot { get; init; }

    public string? StringValue { get; init; }

    public int? IntegerValue { get; init; }
}
