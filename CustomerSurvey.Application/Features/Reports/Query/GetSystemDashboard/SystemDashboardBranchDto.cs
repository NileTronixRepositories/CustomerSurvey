namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed record SystemDashboardBranchDto
{
    public Guid BranchId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string Code { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}