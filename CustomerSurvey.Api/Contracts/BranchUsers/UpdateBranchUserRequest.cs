namespace CustomerSurvey.Api.Contracts.BranchUsers
{
    public sealed class UpdateBranchUserRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }
    }
}