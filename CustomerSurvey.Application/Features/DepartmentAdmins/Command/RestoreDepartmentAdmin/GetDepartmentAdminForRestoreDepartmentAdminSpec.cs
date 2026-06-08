using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.RestoreDepartmentAdmin
{
    internal sealed record DepartmentAdminForRestoreDepartmentAdminDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }
    }

    internal sealed class GetDepartmentAdminForRestoreDepartmentAdminSpec
        : Specification<DepartmentAdmin, DepartmentAdminForRestoreDepartmentAdminDto>
    {
        public GetDepartmentAdminForRestoreDepartmentAdminSpec(Guid departmentAdminId)
        {
            AddCriteria(x => x.Id == departmentAdminId);

            Select(x => new DepartmentAdminForRestoreDepartmentAdminDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId
            });
        }
    }
}
