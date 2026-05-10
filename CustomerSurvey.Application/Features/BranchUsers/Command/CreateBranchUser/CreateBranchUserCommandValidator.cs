using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.CreateBranchUser
{
    internal sealed class CreateBranchUserCommandValidator
        : AbstractValidator<CreateBranchUserCommand>
    {
        public CreateBranchUserCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchUser_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranchUser_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranchUser_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchUser_UserName_Required)
                .MaximumLength(100)
                .WithMessage(ErrorMessage.CreateBranchUser_UserName_MaxLength);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchUser_Email_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranchUser_Email_MaxLength)
                .EmailAddress()
                .WithMessage(ErrorMessage.CreateBranchUser_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.CreateBranchUser_PhoneNumber_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchUser_Password_Required)
                .MinimumLength(8)
                .WithMessage(ErrorMessage.CreateBranchUser_Password_MinLength)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranchUser_Password_MaxLength);

            RuleFor(x => x.RoleIds)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchUser_RoleIds_Required);

            RuleForEach(x => x.RoleIds)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchUser_RoleId_Invalid);
        }
    }
}