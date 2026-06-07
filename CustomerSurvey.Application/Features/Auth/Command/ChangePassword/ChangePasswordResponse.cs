namespace CustomerSurvey.Application.Features.Auth.Command.ChangePassword
{
    public sealed record ChangePasswordResponse
    {
        public Guid ApplicationUserId { get; init; }

        public bool PasswordChanged { get; init; }

        public bool FirstLoginFlag { get; init; }

        public bool PasswordExpiredFlag { get; init; }

        public DateTime PasswordChangedOnUtc { get; init; }
    }
}
