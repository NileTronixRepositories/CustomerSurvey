using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Auth.Command.Login
{
    internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.UserNameOrEmail)
                .NotEmpty()
                .WithMessage(ErrorMessage.Login_UserNameOrEmail_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.Login_UserNameOrEmail_MaxLength);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.Login_Password_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.Login_Password_MaxLength);
        }
    }
}