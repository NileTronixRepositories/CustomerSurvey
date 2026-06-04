namespace CustomerSurvey.Api.Contracts.BranchAreas
{
    public sealed class UpdateBranchAreaRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }
    }
}
