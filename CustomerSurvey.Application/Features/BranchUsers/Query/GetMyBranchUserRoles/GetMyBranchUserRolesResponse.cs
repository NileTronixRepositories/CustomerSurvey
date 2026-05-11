using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetMyBranchUserRoles
{
    public sealed record GetMyBranchUserRolesResponse
    {
        public Guid ApplicationUserId { get; init; }

        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }

        public IReadOnlyCollection<MyBranchUserRoleResponse> Roles { get; init; }
            = Array.Empty<MyBranchUserRoleResponse>();
    }

    public sealed record MyBranchUserRoleResponse
    {
        public Guid RoleId { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}