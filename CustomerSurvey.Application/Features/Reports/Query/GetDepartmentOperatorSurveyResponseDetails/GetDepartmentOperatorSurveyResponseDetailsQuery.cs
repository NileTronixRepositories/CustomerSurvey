using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

public sealed record GetDepartmentOperatorSurveyResponseDetailsQuery
    : IQuery<GetDepartmentOperatorSurveyResponseDetailsResponse>
{
    public Guid OperatorId { get; set; }

    public Guid SurveyResponseId { get; set; }
}
