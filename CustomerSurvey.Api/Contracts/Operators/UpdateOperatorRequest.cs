namespace CustomerSurvey.Api.Contracts.Operators
{
    public sealed class UpdateOperatorRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }
    }
}