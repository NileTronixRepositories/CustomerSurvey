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

        public BranchPaginationCreatedByResponse? CreatedBy { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }

    public sealed record BranchPaginationCreatedByResponse
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}