using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed class GetSystemDashboardOperatorsSpec
    : Specification<Operator, SystemDashboardOperatorDto>
{
    public GetSystemDashboardOperatorsSpec(Guid? departmentId)
    {
        if (departmentId.HasValue)
        {
            AddCriteria(x => x.DepartmentId == departmentId.Value);
        }

        Select(x => new SystemDashboardOperatorDto
        {
            OperatorId = x.Id,
            DepartmentId = x.DepartmentId,
            OperatorNameEn = x.ApplicationUser.NameEn,
            OperatorNameAr = x.ApplicationUser.NameAr,
            DepartmentNameEn = x.Department.NameEn,
            DepartmentNameAr = x.Department.NameAr
        });
    }
}