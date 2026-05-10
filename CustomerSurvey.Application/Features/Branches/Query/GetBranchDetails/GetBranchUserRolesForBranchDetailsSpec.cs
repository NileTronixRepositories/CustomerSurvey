using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    internal sealed record BranchUserRoleForBranchDetailsDto
    {
        public Guid ApplicationUserId { get; init; }

        public Guid RoleId { get; init; }

        public string RoleName { get; init; } = string.Empty;
    }

    internal sealed class GetBranchUserRolesForBranchDetailsSpec
        : Specification<UserRole, BranchUserRoleForBranchDetailsDto>
    {
        public GetBranchUserRolesForBranchDetailsSpec(
            IReadOnlyCollection<Guid> applicationUserIds)
        {
            AddCriteria(x => applicationUserIds.Contains(x.ApplicationUserId));

            AddOrderBy(x => x.Role.Name);

            Select(x => new BranchUserRoleForBranchDetailsDto
            {
                ApplicationUserId = x.ApplicationUserId,
                RoleId = x.RoleId,
                RoleName = x.Role.Name
            });
        }
    }
}