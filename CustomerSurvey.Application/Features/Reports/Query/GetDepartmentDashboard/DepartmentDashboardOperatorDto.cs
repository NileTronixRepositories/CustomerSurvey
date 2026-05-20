namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed record DepartmentDashboardOperatorDto
{
    public Guid OperatorId { get; init; }

    public string OperatorNameEn { get; init; } = string.Empty;

    public string? OperatorNameAr { get; init; }

    public bool IsActive { get; init; }
}
