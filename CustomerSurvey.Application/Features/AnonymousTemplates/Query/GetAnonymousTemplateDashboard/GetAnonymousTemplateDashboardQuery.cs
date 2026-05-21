using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

public sealed class GetAnonymousTemplateDashboardQuery : IQuery<GetAnonymousTemplateDashboardResponse>
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public Guid? AnonymousTemplateId { get; init; }

    public AnonymousTemplateDashboardGroupBy GroupBy { get; init; } = AnonymousTemplateDashboardGroupBy.Day;

    public int TopQuestionsCount { get; init; } = 5;

    public int CriticalResponsesCount { get; init; } = 10;

    public decimal CriticalScoreThreshold { get; init; } = 40m;
}

public enum AnonymousTemplateDashboardGroupBy
{
    Day = 1,
    Month = 2
}
