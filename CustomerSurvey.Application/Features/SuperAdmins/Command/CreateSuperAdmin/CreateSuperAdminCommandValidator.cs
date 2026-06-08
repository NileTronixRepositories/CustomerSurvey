using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin
{
    internal sealed class CreateSuperAdminCommandValidator
        : AbstractValidator<CreateSuperAdminCommand>
    {
        public CreateSuperAdminCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateSuperAdmin_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateSuperAdmin_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateSuperAdmin_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateSuperAdmin_UserName_Required)
                .MaximumLength(100)
                .WithMessage(ErrorMessage.CreateSuperAdmin_UserName_MaxLength);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateSuperAdmin_Email_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateSuperAdmin_Email_MaxLength)
                .EmailAddress()
                .WithMessage(ErrorMessage.CreateSuperAdmin_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.CreateSuperAdmin_PhoneNumber_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateSuperAdmin_Password_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateSuperAdmin_Password_MaxLength);
        }
    }
}
