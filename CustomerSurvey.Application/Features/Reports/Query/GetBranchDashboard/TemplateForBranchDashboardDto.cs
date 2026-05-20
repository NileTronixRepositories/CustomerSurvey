namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed record TemplateForBranchDashboardDto
{
    public Guid TemplateId { get; init; }

    public Guid BranchId { get; init; }

    public DateTime CreatedOnUtc { get; init; }
}