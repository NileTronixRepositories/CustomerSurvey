using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed class GetTemplateForBranchDashboardSpec
    : Specification<Template, TemplateForBranchDashboardDto>
{
    public GetTemplateForBranchDashboardSpec(Guid templateId, Guid branchId)
    {
        AddCriteria(x =>
            x.Id == templateId &&
            x.BranchId == branchId);

        Select(x => new TemplateForBranchDashboardDto
        {
            TemplateId = x.Id,
            BranchId = x.BranchId,
            CreatedOnUtc = x.CreatedOnUtc
        });
    }
}