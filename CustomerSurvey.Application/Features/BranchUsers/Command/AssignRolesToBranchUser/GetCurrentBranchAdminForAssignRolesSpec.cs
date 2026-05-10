using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.AssignRolesToBranchUser
{
    internal sealed record CurrentBranchAdminForAssignRolesDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForAssignRolesSpec
        : Specification<BranchAdmin, CurrentBranchAdminForAssignRolesDto>
    {
        public GetCurrentBranchAdminForAssignRolesSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForAssignRolesDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}