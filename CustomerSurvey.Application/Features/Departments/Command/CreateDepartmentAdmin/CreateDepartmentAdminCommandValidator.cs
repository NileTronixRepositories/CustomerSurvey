using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.CreateDepartmentAdmin
{
    internal sealed class CreateDepartmentAdminCommandValidator
        : AbstractValidator<CreateDepartmentAdminCommand>
    {
        public CreateDepartmentAdminCommandValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_DepartmentId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_UserName_Required)
                .MaximumLength(100)
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_UserName_MaxLength);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_Email_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_Email_MaxLength)
                .EmailAddress()
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_PhoneNumber_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_Password_Required)
                .MinimumLength(8)
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_Password_MinLength)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateDepartmentAdmin_Password_MaxLength);
        }
    }
}