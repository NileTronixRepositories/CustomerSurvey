using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Auth.Command.ChangePassword
{
    public sealed record ChangePasswordCommand : ICommand<ChangePasswordResponse>
    {
        public Guid ApplicationUserId { get; init; }

        public string NewPassword { get; init; } = string.Empty;

        public string ConfirmNewPassword { get; init; } = string.Empty;
    }
}
