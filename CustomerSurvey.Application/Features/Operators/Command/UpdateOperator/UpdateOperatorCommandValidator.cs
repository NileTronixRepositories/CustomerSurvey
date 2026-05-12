using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.UpdateOperator
{
    internal sealed class UpdateOperatorCommandValidator
        : AbstractValidator<UpdateOperatorCommand>
    {
        public UpdateOperatorCommandValidator()
        {
            RuleFor(x => x.OperatorId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateOperator_OperatorId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateOperator_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateOperator_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateOperator_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateOperator_Email_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateOperator_Email_MaxLength)
                .EmailAddress()
                .WithMessage(ErrorMessage.UpdateOperator_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.UpdateOperator_PhoneNumber_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}