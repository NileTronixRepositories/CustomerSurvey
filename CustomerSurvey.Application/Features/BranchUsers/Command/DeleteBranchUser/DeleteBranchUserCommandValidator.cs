using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.DeleteBranchUser
{
    internal sealed class DeleteBranchUserCommandValidator
        : AbstractValidator<DeleteBranchUserCommand>
    {
        public DeleteBranchUserCommandValidator()
        {
            RuleFor(x => x.ApplicationUserId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeleteBranchUser_ApplicationUserId_Required);
        }
    }
}