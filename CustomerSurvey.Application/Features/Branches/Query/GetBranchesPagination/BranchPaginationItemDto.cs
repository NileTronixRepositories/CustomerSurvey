using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesPagination
{
    internal sealed record BranchPaginationItemDto
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;

        public string? Address { get; init; }

        public bool IsActive { get; init; }

        public Guid CreatedByApplicationUserId { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}