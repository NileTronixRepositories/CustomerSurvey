using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesPagination
{
    public sealed record BranchPaginationItemResponse
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;

        public string? Address { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}