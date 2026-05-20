using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed class GetDepartmentDashboardOperatorsSpec
    : Specification<Operator, DepartmentDashboardOperatorDto>
{
    public GetDepartmentDashboardOperatorsSpec(Guid departmentId)
    {
        AddCriteria(x => x.DepartmentId == departmentId);

        Select(x => new DepartmentDashboardOperatorDto
        {
            OperatorId = x.Id,
            OperatorNameEn = x.ApplicationUser.NameEn,
            OperatorNameAr = x.ApplicationUser.NameAr,
            IsActive = x.ApplicationUser.IsActive
        });
    }
}
