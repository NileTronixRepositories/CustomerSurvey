using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed class GetBranchTemplatesForDashboardSpec
    : Specification<Template, TemplateDashboardItemDto>
{
    public GetBranchTemplatesForDashboardSpec(Guid branchId)
    {
        AddCriteria(x => x.BranchId == branchId);

        Select(x => new TemplateDashboardItemDto
        {
            TemplateId = x.Id,
            NameEn = x.NameEn,
            NameAr = x.NameAr,
            IsActive = x.IsActive
        });
    }
}