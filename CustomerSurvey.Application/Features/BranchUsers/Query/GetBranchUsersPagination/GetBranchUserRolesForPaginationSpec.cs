using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetBranchUsersPagination
{
    internal sealed record BranchUserRoleForPaginationDto
    {
        public Guid ApplicationUserId { get; init; }

        public Guid RoleId { get; init; }

        public string RoleName { get; init; } = string.Empty;
    }

    internal sealed class GetBranchUserRolesForPaginationSpec
        : Specification<UserRole, BranchUserRoleForPaginationDto>
    {
        public GetBranchUserRolesForPaginationSpec(IReadOnlyCollection<Guid> applicationUserIds)
        {
            var ids = applicationUserIds
                .Distinct()
                .ToArray();

            AddCriteria(x => ids.Contains(x.ApplicationUserId));

            Select(x => new BranchUserRoleForPaginationDto
            {
                ApplicationUserId = x.ApplicationUserId,
                RoleId = x.RoleId,
                RoleName = x.Role.Name
            });
        }
    }
}