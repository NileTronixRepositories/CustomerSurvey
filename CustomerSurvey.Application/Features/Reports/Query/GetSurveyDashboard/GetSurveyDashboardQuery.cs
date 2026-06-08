using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

public sealed class GetSurveyDashboardQuery : IQuery<SurveyDashboardResponse>
{
    public Guid? BranchId { get; init; }

    public SurveyDashboardSource Source { get; init; } = SurveyDashboardSource.All;

    public Guid? TemplateId { get; init; }

    public Guid? AnonymousTemplateId { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public DashboardGroupBy GroupBy { get; init; } = DashboardGroupBy.Day;

    public ScoreCalculationMode ScoreCalculationMode { get; init; } = ScoreCalculationMode.RootQuestions;

    public int TopQuestionsCount { get; init; } = 5;

    public int CriticalResponsesCount { get; init; } = 10;

    public decimal CriticalScoreThreshold { get; init; } = 40m;
}

public enum SurveyDashboardSource
{
    All = 1,
    Internal = 2,
    Anonymous = 3
}

public enum SurveyDashboardTemplateKind
{
    Authorized = 1,
    Anonymous = 2
}

public enum DashboardGroupBy
{
    Day = 1,
    Month = 2
}
