using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetBranchUsersPagination
{
    internal sealed record BranchUserPaginationCreatorDto
    {
        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}