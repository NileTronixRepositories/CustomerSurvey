using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

public sealed class GetDepartmentDashboardQuery : IQuery<GetDepartmentDashboardResponse>
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public Guid? TemplateId { get; init; }

    public DepartmentDashboardGroupBy GroupBy { get; init; } = DepartmentDashboardGroupBy.Day;

    public int TopQuestionsCount { get; init; } = 5;

    public int CriticalResponsesCount { get; init; } = 10;

    public decimal CriticalScoreThreshold { get; init; } = 40m;
}

public enum DepartmentDashboardGroupBy
{
    Day = 1,
    Month = 2
}
