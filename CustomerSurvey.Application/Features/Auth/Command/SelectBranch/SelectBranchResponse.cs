namespace CustomerSurvey.Application.Features.Auth.Command.SelectBranch
{
    public sealed record SelectBranchResponse
    {
        public string Token { get; init; } = string.Empty;

        public string UserType { get; init; } = string.Empty;

        public Guid ActiveBranchId { get; init; }

        public SelectedBranchResponse SelectedBranch { get; init; } = new();
    }

    public sealed record SelectedBranchResponse
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;
    }
}
