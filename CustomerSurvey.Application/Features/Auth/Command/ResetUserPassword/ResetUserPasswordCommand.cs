using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword
{
    public sealed record ResetUserPasswordCommand : ICommand<ResetUserPasswordResponse>
    {
        public Guid ApplicationUserId { get; init; }

        public string NewPassword { get; init; } = string.Empty;

        public string ConfirmNewPassword { get; init; } = string.Empty;
    }
}
