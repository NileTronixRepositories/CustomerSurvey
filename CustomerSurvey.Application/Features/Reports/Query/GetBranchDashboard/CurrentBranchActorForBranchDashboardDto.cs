namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed record CurrentBranchActorForBranchDashboardDto
{
    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }
}