namespace CustomerSurvey.Api.Contracts.Auth
{
    public sealed class ResetUserPasswordRequest
    {
        public string NewPassword { get; init; } = string.Empty;

        public string ConfirmNewPassword { get; init; } = string.Empty;
    }
}
