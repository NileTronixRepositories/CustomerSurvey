using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranchAdmin
{
    public sealed record CreateBranchAdminResponse
    {
        public Guid ApplicationUserId { get; init; }

        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
    }
}