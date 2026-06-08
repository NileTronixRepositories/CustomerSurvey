using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword
{
    internal sealed class ResetUserPasswordCommandValidator
        : AbstractValidator<ResetUserPasswordCommand>
    {
        public ResetUserPasswordCommandValidator()
        {
            RuleFor(x => x.ApplicationUserId)
                .NotEmpty()
                .WithMessage(ErrorMessage.ResetUserPassword_ApplicationUserId_Required);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(ErrorMessage.ResetUserPassword_NewPassword_Required)
                .MinimumLength(8)
                .WithMessage(ErrorMessage.ResetUserPassword_NewPassword_MinLength)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.ResetUserPassword_NewPassword_MaxLength);

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage(ErrorMessage.ResetUserPassword_ConfirmNewPassword_Required)
                .Equal(x => x.NewPassword)
                .WithMessage(ErrorMessage.ResetUserPassword_Passwords_NotMatch);
        }
    }
}
