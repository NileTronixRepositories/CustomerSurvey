using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.RestoreBranchUser
{
    internal sealed class RestoreBranchUserCommandValidator
       : AbstractValidator<RestoreBranchUserCommand>
    {
        public RestoreBranchUserCommandValidator()
        {
            RuleFor(x => x.ApplicationUserId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreBranchUser_ApplicationUserId_Required);
        }
    }
}