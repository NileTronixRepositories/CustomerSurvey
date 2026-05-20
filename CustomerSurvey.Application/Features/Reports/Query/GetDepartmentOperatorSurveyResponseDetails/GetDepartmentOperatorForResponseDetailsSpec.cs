using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed class GetDepartmentOperatorForResponseDetailsSpec
    : Specification<Operator, DepartmentOperatorForResponseDetailsDto>
{
    public GetDepartmentOperatorForResponseDetailsSpec(
        Guid operatorId,
        Guid departmentId)
    {
        AddCriteria(x =>
            x.Id == operatorId &&
            x.DepartmentId == departmentId);

        Select(x => new DepartmentOperatorForResponseDetailsDto
        {
            OperatorId = x.Id,
            DepartmentId = x.DepartmentId
        });
    }
}
