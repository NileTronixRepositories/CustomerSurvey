using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranch
{
    public sealed record CreateBranchResponse
    {
        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string Code { get; init; } = string.Empty;
    }
}