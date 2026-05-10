using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.AssignRolesToBranchUser
{
    public sealed record AssignRolesToBranchUserResponse
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }

        public IReadOnlyCollection<AssignedBranchUserRoleResponse> Roles { get; init; }
            = Array.Empty<AssignedBranchUserRoleResponse>();
    }

    public sealed record AssignedBranchUserRoleResponse
    {
        public Guid RoleId { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}