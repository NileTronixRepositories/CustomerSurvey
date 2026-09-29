using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardComplaints;

public sealed class GetSurveyDashboardComplaintsQuery
    : SearchParameters, IQuery<SurveyDashboardComplaintsResponse>, ISurveyDashboardFilterRequest
{
    public Guid? BranchId { get; init; }
    public SurveyDashboardSource Source { get; init; } = SurveyDashboardSource.All;
    public Guid? TemplateId { get; init; }
    public Guid? AnonymousTemplateId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}
