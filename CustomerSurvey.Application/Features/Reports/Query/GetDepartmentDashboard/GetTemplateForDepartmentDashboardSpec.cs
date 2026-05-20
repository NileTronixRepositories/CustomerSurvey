using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed class GetTemplateForDepartmentDashboardSpec
    : Specification<Template, TemplateForDepartmentDashboardDto>
{
    public GetTemplateForDepartmentDashboardSpec(Guid templateId)
    {
        AddCriteria(x => x.Id == templateId);

        Select(x => new TemplateForDepartmentDashboardDto
        {
            TemplateId = x.Id
        });
    }
}
