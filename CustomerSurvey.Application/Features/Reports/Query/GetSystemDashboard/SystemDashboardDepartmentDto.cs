namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed record SystemDashboardDepartmentDto
{
    public Guid DepartmentId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public bool IsActive { get; init; }
}