namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed record CurrentDepartmentAdminForDepartmentDashboardDto
{
    public Guid DepartmentAdminId { get; init; }

    public Guid DepartmentId { get; init; }

    public string DepartmentNameEn { get; init; } = string.Empty;

    public string? DepartmentNameAr { get; init; }
}
