using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.RestoreBranchUser
{
    public sealed record RestoreBranchUserCommand
         : ICommand<RestoreBranchUserResponse>
    {
        public Guid ApplicationUserId { get; init; }
    }
}