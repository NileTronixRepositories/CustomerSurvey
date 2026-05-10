using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.AssignRolesToBranchUser
{
    public sealed record AssignRolesToBranchUserCommand
         : ICommand<AssignRolesToBranchUserResponse>
    {
        public Guid ApplicationUserId { get; init; }

        public IReadOnlyCollection<Guid> RoleIds { get; init; } = Array.Empty<Guid>();
    }
}