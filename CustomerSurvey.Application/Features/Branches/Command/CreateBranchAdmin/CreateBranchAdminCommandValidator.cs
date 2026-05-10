using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranchAdmin
{
    internal sealed class CreateBranchAdminCommandValidator
        : AbstractValidator<CreateBranchAdminCommand>
    {
        public CreateBranchAdminCommandValidator()
        {
            RuleFor(x => x.BranchId)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchAdmin_BranchId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchAdmin_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranchAdmin_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranchAdmin_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchAdmin_UserName_Required)
                .MaximumLength(100)
                .WithMessage(ErrorMessage.CreateBranchAdmin_UserName_MaxLength);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchAdmin_Email_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranchAdmin_Email_MaxLength)
                .EmailAddress()
                .WithMessage(ErrorMessage.CreateBranchAdmin_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.CreateBranchAdmin_PhoneNumber_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchAdmin_Password_Required)
                .MinimumLength(8)
                .WithMessage(ErrorMessage.CreateBranchAdmin_Password_MinLength)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranchAdmin_Password_MaxLength);
        }
    }
}