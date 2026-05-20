using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed class GetCurrentDepartmentAdminForDepartmentDashboardSpec
    : Specification<DepartmentAdmin, CurrentDepartmentAdminForDepartmentDashboardDto>
{
    public GetCurrentDepartmentAdminForDepartmentDashboardSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new CurrentDepartmentAdminForDepartmentDashboardDto
        {
            DepartmentAdminId = x.Id,
            DepartmentId = x.DepartmentId,
            DepartmentNameEn = x.Department.NameEn,
            DepartmentNameAr = x.Department.NameAr
        });
    }
}
