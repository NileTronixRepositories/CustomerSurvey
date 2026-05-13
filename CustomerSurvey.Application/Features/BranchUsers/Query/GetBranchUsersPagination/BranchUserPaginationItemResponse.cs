namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetBranchUsersPagination
{
    public sealed record BranchUserPaginationItemResponse
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }

        public IReadOnlyCollection<BranchUserPaginationRoleResponse> Roles { get; init; }
            = Array.Empty<BranchUserPaginationRoleResponse>();
    }

    public sealed record BranchUserPaginationRoleResponse
    {
        public Guid RoleId { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}