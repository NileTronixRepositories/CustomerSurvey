namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed record TemplateDashboardItemDto
{
    public Guid TemplateId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public bool IsActive { get; init; }
}