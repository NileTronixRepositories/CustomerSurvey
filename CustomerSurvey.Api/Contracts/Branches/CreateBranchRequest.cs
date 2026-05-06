namespace CustomerSurvey.Api.Contracts.Branches
{
    public sealed class CreateBranchRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;

        public string? Address { get; init; }
    }
}