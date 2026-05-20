using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

public sealed class GetSystemDashboardQuery : IQuery<GetSystemDashboardResponse>
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public Guid? BranchId { get; init; }

    public Guid? DepartmentId { get; init; }

    public SystemDashboardGroupBy GroupBy { get; init; } = SystemDashboardGroupBy.Day;

    public decimal CriticalScoreThreshold { get; init; } = 40m;

    public int CriticalResponsesCount { get; init; } = 10;

    public int TopTemplatesCount { get; init; } = 10;
}

public enum SystemDashboardGroupBy
{
    Day = 1,
    Month = 2
}