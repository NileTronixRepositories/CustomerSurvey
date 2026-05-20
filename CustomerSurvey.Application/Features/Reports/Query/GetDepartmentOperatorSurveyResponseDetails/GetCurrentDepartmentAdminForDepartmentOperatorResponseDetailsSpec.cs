using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed class GetCurrentDepartmentAdminForDepartmentOperatorResponseDetailsSpec
    : Specification<DepartmentAdmin, DepartmentAdminForDepartmentOperatorResponseDetailsDto>
{
    public GetCurrentDepartmentAdminForDepartmentOperatorResponseDetailsSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new DepartmentAdminForDepartmentOperatorResponseDetailsDto
        {
            DepartmentAdminId = x.Id,
            DepartmentId = x.DepartmentId
        });
    }
}
