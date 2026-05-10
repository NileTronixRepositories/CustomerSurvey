using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.AssignRolesToBranchUser
{
    internal sealed class AssignRolesToBranchUserCommandValidator
      : AbstractValidator<AssignRolesToBranchUserCommand>
    {
        public AssignRolesToBranchUserCommandValidator()
        {
            RuleFor(x => x.ApplicationUserId)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignRolesToBranchUser_ApplicationUserId_Required);

            RuleFor(x => x.RoleIds)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignRolesToBranchUser_RoleIds_Required);

            RuleForEach(x => x.RoleIds)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignRolesToBranchUser_RoleId_Invalid);
        }
    }
}