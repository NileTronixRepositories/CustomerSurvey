namespace CustomerSurvey.Api.Contracts.Branches
{
    public sealed class CreateBranchAdminRequest
    {
        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public string Password { get; init; } = string.Empty;
    }
}