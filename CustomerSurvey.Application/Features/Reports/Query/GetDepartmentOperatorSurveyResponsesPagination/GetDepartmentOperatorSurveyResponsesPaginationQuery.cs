using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

public sealed class GetDepartmentOperatorSurveyResponsesPaginationQuery
    : SearchParameters, IQuery<Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>>
{
    public Guid OperatorId { get; set; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public Guid? TemplateId { get; init; }

    public decimal? MinScorePercentage { get; init; }

    public decimal? MaxScorePercentage { get; init; }

    public bool? HasComplaint { get; init; }

    public bool? HasVoice { get; init; }
}
