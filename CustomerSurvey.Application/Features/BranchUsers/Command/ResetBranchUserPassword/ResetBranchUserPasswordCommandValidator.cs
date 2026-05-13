using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.ResetBranchUserPassword
{
    internal sealed class ResetBranchUserPasswordCommandValidator
         : AbstractValidator<ResetBranchUserPasswordCommand>
    {
        public ResetBranchUserPasswordCommandValidator()
        {
            RuleFor(x => x.ApplicationUserId)
                .NotEmpty()
                .WithMessage(ErrorMessage.ResetBranchUserPassword_ApplicationUserId_Required);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(ErrorMessage.ResetBranchUserPassword_NewPassword_Required)
                .MinimumLength(8)
                .WithMessage(ErrorMessage.ResetBranchUserPassword_NewPassword_MinLength)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.ResetBranchUserPassword_NewPassword_MaxLength);
        }
    }
}