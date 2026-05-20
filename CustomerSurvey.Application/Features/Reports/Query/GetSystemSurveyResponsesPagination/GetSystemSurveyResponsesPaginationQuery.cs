using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;

public sealed class GetSystemSurveyResponsesPaginationQuery
    : SearchParameters, IQuery<Pagination<SystemSurveyResponsePaginationItemResponse>>
{
    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? TemplateId { get; set; }

    public decimal? MinScorePercentage { get; set; }

    public decimal? MaxScorePercentage { get; set; }

    public bool? HasComplaint { get; set; }

    public bool? HasVoice { get; set; }
}