using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed class GetDepartmentDashboardAssignedTemplatesSpec
    : Specification<OperatorTemplate, DepartmentDashboardAssignedTemplateDto>
{
    public GetDepartmentDashboardAssignedTemplatesSpec(Guid departmentId, Guid? templateId = null)
    {
        AddCriteria(x => x.Operator.DepartmentId == departmentId);
        if (templateId.HasValue) AddCriteria(x => x.TemplateId == templateId);

        Select(x => new DepartmentDashboardAssignedTemplateDto
        {
            TemplateId = x.TemplateId,
            TemplateNameEn = x.Template.NameEn,
            TemplateNameAr = x.Template.NameAr,
            BranchId = x.Template.BranchId,
            BranchNameEn = x.Template.Branch.NameEn,
            BranchNameAr = x.Template.Branch.NameAr,
            IsActive = x.Template.IsActive
        });
    }
}
