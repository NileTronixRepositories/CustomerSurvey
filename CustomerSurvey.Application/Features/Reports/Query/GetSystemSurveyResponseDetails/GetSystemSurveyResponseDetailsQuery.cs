using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

public sealed record GetSystemSurveyResponseDetailsQuery
    : IQuery<GetSystemSurveyResponseDetailsResponse>
{
    public Guid SurveyResponseId { get; init; }
}