using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.UpdateBranchUser
{
    internal sealed class UpdateBranchUserCommandValidator
        : AbstractValidator<UpdateBranchUserCommand>
    {
        public UpdateBranchUserCommandValidator()
        {
            RuleFor(x => x.ApplicationUserId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateBranchUser_ApplicationUserId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateBranchUser_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateBranchUser_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateBranchUser_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateBranchUser_Email_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateBranchUser_Email_MaxLength)
                .EmailAddress()
                .WithMessage(ErrorMessage.UpdateBranchUser_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.UpdateBranchUser_PhoneNumber_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}