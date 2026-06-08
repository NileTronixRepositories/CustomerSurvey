using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Operators.Command.RestoreOperator
{
    internal sealed record DepartmentAdminForRestoreOperatorDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetCurrentDepartmentAdminForRestoreOperatorSpec
        : Specification<DepartmentAdmin, DepartmentAdminForRestoreOperatorDto>
    {
        public GetCurrentDepartmentAdminForRestoreOperatorSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new DepartmentAdminForRestoreOperatorDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}
