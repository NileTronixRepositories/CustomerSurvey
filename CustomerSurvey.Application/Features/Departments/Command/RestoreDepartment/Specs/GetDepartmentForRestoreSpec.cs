using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Departments.Command.RestoreDepartment.Specs
{
    internal sealed class GetDepartmentForRestoreSpec
        : Specification<Department>
    {
        public GetDepartmentForRestoreSpec(Guid departmentId)
        {
            AddCriteria(x => x.Id == departmentId);
        }
    }
}
