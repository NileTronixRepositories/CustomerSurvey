using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyResponsesPagination;

public sealed class GetSurveyResponsesPaginationQuery
    : SearchParameters, IQuery<Pagination<SurveyResponsePaginationItemResponse>>
{
    public SurveyDashboardSource Source { get; init; } = SurveyDashboardSource.All;

    public Guid? BranchId { get; init; }

    public Guid? TemplateId { get; init; }

    public Guid? AnonymousTemplateId { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public ScoreCalculationMode ScoreCalculationMode { get; init; } = ScoreCalculationMode.RootQuestions;

    public SatisfactionCategory? SatisfactionCategory { get; init; }

    public bool? IsScored { get; init; }

    public bool? HasComplaint { get; init; }

    public bool? HasVoice { get; init; }

    public Guid? QuestionId { get; init; }

    public string? CustomInputName { get; init; }

    public string? CustomInputLabelEn { get; init; }

    public string? CustomInputLabelAr { get; init; }

    public bool? CustomInputLabelEnIsNull { get; init; }

    public bool? CustomInputLabelArIsNull { get; init; }

    public TemplateCustomInputType? CustomInputType { get; init; }

    public string? CustomInputValue { get; init; }
}
