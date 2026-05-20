using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed class GetSystemDashboardTemplatesSpec
    : Specification<Template, SystemDashboardTemplateDto>
{
    public GetSystemDashboardTemplatesSpec(Guid? branchId)
    {
        if (branchId.HasValue)
        {
            AddCriteria(x => x.BranchId == branchId.Value);
        }

        Select(x => new SystemDashboardTemplateDto
        {
            TemplateId = x.Id,
            BranchId = x.BranchId,
            TemplateNameEn = x.NameEn,
            TemplateNameAr = x.NameAr,
            BranchNameEn = x.Branch.NameEn,
            BranchNameAr = x.Branch.NameAr,
            IsActive = x.IsActive
        });
    }
}