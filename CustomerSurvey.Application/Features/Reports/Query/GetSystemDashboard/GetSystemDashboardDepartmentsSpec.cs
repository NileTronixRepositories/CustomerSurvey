using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed class GetSystemDashboardDepartmentsSpec
    : Specification<Department, SystemDashboardDepartmentDto>
{
    public GetSystemDashboardDepartmentsSpec(Guid? departmentId)
    {
        if (departmentId.HasValue)
        {
            AddCriteria(x => x.Id == departmentId.Value);
        }

        Select(x => new SystemDashboardDepartmentDto
        {
            DepartmentId = x.Id,
            NameEn = x.NameEn,
            NameAr = x.NameAr,
            IsActive = x.IsActive
        });
    }
}