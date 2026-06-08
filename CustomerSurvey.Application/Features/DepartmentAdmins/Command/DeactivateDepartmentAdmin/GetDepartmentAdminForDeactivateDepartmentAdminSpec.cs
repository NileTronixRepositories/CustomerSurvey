using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.DeactivateDepartmentAdmin
{
    internal sealed record DepartmentAdminForDeactivateDepartmentAdminDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }
    }

    internal sealed class GetDepartmentAdminForDeactivateDepartmentAdminSpec
        : Specification<DepartmentAdmin, DepartmentAdminForDeactivateDepartmentAdminDto>
    {
        public GetDepartmentAdminForDeactivateDepartmentAdminSpec(Guid departmentAdminId)
        {
            AddCriteria(x => x.Id == departmentAdminId);

            Select(x => new DepartmentAdminForDeactivateDepartmentAdminDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId
            });
        }
    }
}
