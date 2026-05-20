using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

internal sealed class GetCurrentDepartmentAdminForDepartmentOperatorResponsesSpec
    : Specification<DepartmentAdmin, DepartmentAdminForDepartmentOperatorResponsesDto>
{
    public GetCurrentDepartmentAdminForDepartmentOperatorResponsesSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new DepartmentAdminForDepartmentOperatorResponsesDto
        {
            DepartmentAdminId = x.Id,
            DepartmentId = x.DepartmentId
        });
    }
}
