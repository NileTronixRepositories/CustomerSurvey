using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Auth.Command.ChangePassword
{
    internal sealed class ChangePasswordCommandValidator
        : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(ErrorMessage.ChangePassword_NewPassword_Required)
                .MinimumLength(8)
                .WithMessage(ErrorMessage.ChangePassword_NewPassword_MinLength)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.ChangePassword_NewPassword_MaxLength);

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage(ErrorMessage.ChangePassword_ConfirmNewPassword_Required)
                .Equal(x => x.NewPassword)
                .WithMessage(ErrorMessage.ChangePassword_ConfirmNewPassword_NotMatched);
        }
    }
}
