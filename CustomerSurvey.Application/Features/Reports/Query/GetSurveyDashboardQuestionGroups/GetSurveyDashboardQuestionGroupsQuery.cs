using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardQuestionGroups;

public sealed class GetSurveyDashboardQuestionGroupsQuery
    : IQuery<SurveyDashboardQuestionGroupsResponse>, ISurveyDashboardFilterRequest
{
    public Guid? BranchId { get; init; }
    public SurveyDashboardSource Source { get; init; } = SurveyDashboardSource.All;
    public Guid? TemplateId { get; init; }
    public Guid? AnonymousTemplateId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public ScoreCalculationMode ScoreCalculationMode { get; init; } = ScoreCalculationMode.RootQuestions;
}
