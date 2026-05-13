using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.CreateOperator
{
    internal sealed class CreateOperatorCommandValidator
        : AbstractValidator<CreateOperatorCommand>
    {
        public CreateOperatorCommandValidator()
        {
            RuleFor(x => x.DepartmentId)
                .Must(x => !x.HasValue || x.Value != Guid.Empty)
                .WithMessage(ErrorMessage.CreateOperator_DepartmentId_Invalid);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateOperator_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateOperator_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateOperator_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateOperator_UserName_Required)
                .MaximumLength(100)
                .WithMessage(ErrorMessage.CreateOperator_UserName_MaxLength);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateOperator_Email_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateOperator_Email_MaxLength)
                .EmailAddress()
                .WithMessage(ErrorMessage.CreateOperator_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.CreateOperator_PhoneNumber_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateOperator_Password_Required)
                .MinimumLength(8)
                .WithMessage(ErrorMessage.CreateOperator_Password_MinLength)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateOperator_Password_MaxLength);
        }
    }
}