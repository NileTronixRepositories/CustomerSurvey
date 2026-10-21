using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Shared;

internal sealed class DepartmentHistoricalTemplateIdsSpec : Specification<SurveyResponse, Guid>
{
    public DepartmentHistoricalTemplateIdsSpec(Guid departmentId, Guid? branchId = null)
    {
        UseNoTracking();
        AddCriteria(x => x.Operator.DepartmentId == departmentId);
        if (branchId.HasValue) AddCriteria(x => x.Template.BranchId == branchId);
        Select(x => x.TemplateId);
        EnableDistinct();
    }
}

internal sealed class DepartmentAssignedTemplateIdsSpec : Specification<OperatorTemplate, Guid>
{
    public DepartmentAssignedTemplateIdsSpec(Guid departmentId, Guid? branchId = null)
    {
        UseNoTracking();
        AddCriteria(x => x.Operator.DepartmentId == departmentId);
        if (branchId.HasValue) AddCriteria(x => x.Template.BranchId == branchId);
        Select(x => x.TemplateId);
        EnableDistinct();
    }
}
