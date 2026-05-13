using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.ResetBranchUserPassword
{
    public sealed record ResetBranchUserPasswordCommand
       : ICommand<ResetBranchUserPasswordResponse>
    {
        public Guid ApplicationUserId { get; init; }

        public string NewPassword { get; init; } = string.Empty;
    }
}