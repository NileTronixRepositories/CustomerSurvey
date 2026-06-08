namespace CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword
{
    public sealed record ResetUserPasswordResponse
    {
        public Guid ApplicationUserId { get; init; }

        public bool PasswordChanged { get; init; }

        public bool MustChangePasswordOnNextLogin { get; init; }

        public DateTime PasswordChangedOnUtc { get; init; }
    }
}
