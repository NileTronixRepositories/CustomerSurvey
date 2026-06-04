using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.UpdateBranchArea
{
    internal sealed class UpdateBranchAreaCommandValidator : AbstractValidator<UpdateBranchAreaCommand>
    {
        public UpdateBranchAreaCommandValidator()
        {
            RuleFor(x => x.BranchAreaId)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignBranchAreaBranches_BranchArea_NotFound);

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
