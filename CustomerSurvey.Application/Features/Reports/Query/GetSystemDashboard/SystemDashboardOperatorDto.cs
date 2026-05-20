namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed record SystemDashboardOperatorDto
{
    public Guid OperatorId { get; init; }

    public Guid DepartmentId { get; init; }

    public string OperatorNameEn { get; init; } = string.Empty;

    public string? OperatorNameAr { get; init; }

    public string DepartmentNameEn { get; init; } = string.Empty;

    public string? DepartmentNameAr { get; init; }
}