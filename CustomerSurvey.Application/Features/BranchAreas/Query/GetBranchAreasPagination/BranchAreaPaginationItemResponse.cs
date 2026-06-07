using CustomerSurvey.Application.Features.BranchAreas.Shared;

namespace CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreasPagination
{
    public sealed record BranchAreaPaginationItemResponse
    {
        public Guid BranchAreaId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }

        public IReadOnlyCollection<BranchAreaBranchItemResponse> Branches { get; init; } =
            Array.Empty<BranchAreaBranchItemResponse>();
    }
}
