namespace CustomerSurvey.Api.Contracts.Auth
{
    public sealed class ChangePasswordRequest
    {
        public string NewPassword { get; init; } = string.Empty;

        public string ConfirmNewPassword { get; init; } = string.Empty;
    }
}
