using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

internal sealed class GetDepartmentOperatorForResponsesSpec
    : Specification<Operator, DepartmentOperatorForResponsesDto>
{
    public GetDepartmentOperatorForResponsesSpec(
        Guid operatorId,
        Guid departmentId)
    {
        AddCriteria(x =>
            x.Id == operatorId &&
            x.DepartmentId == departmentId);

        Select(x => new DepartmentOperatorForResponsesDto
        {
            OperatorId = x.Id,
            DepartmentId = x.DepartmentId
        });
    }
}
