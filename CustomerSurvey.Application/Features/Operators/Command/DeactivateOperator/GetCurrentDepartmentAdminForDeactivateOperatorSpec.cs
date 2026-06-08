using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Operators.Command.DeactivateOperator
{
    internal sealed record DepartmentAdminForDeactivateOperatorDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetCurrentDepartmentAdminForDeactivateOperatorSpec
        : Specification<DepartmentAdmin, DepartmentAdminForDeactivateOperatorDto>
    {
        public GetCurrentDepartmentAdminForDeactivateOperatorSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new DepartmentAdminForDeactivateOperatorDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}
