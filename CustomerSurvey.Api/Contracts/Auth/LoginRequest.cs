namespace CustomerSurvey.Api.Contracts.Auth
{
    public sealed class LoginRequest
    {
        public string UserNameOrEmail { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}