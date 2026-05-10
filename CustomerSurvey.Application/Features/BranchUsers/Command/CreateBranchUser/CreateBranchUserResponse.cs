using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.CreateBranchUser
{
    public sealed record CreateBranchUserResponse
    {
        public Guid ApplicationUserId { get; init; }

        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public IReadOnlyCollection<CreateBranchUserRoleResponse> Roles { get; init; }
            = Array.Empty<CreateBranchUserRoleResponse>();
    }

    public sealed record CreateBranchUserRoleResponse
    {
        public Guid RoleId { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}