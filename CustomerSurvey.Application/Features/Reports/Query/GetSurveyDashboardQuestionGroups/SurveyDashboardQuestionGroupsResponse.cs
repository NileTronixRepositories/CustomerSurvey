using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardQuestionGroups;

public sealed record SurveyDashboardQuestionGroupsResponse
{
    public SurveyDashboardAppliedFiltersResponse AppliedFilters { get; init; } = new();

    public IReadOnlyCollection<SurveyDashboardQuestionGroupItemResponse> QuestionGroups { get; init; }
        = Array.Empty<SurveyDashboardQuestionGroupItemResponse>();
}

public sealed record SurveyDashboardQuestionGroupItemResponse
{
    public Guid TemplateId { get; init; }
    public SurveyDashboardTemplateKind TemplateKind { get; init; }
    public string TemplateNameEn { get; init; } = string.Empty;
    public string? TemplateNameAr { get; init; }
    public Guid QuestionGroupId { get; init; }
    public string QuestionGroupNameEn { get; init; } = string.Empty;
    public string? QuestionGroupNameAr { get; init; }
    public int QuestionsCount { get; init; }
    public int ScorableQuestionsCount { get; init; }
    public int TotalResponses { get; init; }
    public int ScoredResponsesCount { get; init; }
    public int ScoredItemsCount { get; init; }
    public decimal? AverageScoreValue { get; init; }
    public decimal? AverageScorePercentage { get; init; }
}
