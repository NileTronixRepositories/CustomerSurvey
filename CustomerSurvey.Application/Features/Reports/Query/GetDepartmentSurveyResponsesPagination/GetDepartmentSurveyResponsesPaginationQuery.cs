using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination;

public sealed class GetDepartmentSurveyResponsesPaginationQuery
    : SearchParameters, IQuery<Pagination<DepartmentSurveyResponsePaginationItemResponse>>
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public Guid? TemplateId { get; init; }

    public SatisfactionCategory? SatisfactionCategory { get; init; }

    public bool? IsScored { get; init; }

    public decimal? MinScorePercentage { get; init; }

    public decimal? MaxScorePercentage { get; init; }

    public bool? HasComplaint { get; init; }

    public bool? HasVoice { get; init; }

    public Guid? QuestionId { get; init; }

    public string? CustomInputName { get; init; }

    public TemplateCustomInputType? CustomInputType { get; init; }

    public string? CustomInputValue { get; init; }
}
