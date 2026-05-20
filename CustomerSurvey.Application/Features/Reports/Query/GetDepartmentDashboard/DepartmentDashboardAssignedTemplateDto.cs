namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed record DepartmentDashboardAssignedTemplateDto
{
    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public bool IsActive { get; init; }
}
