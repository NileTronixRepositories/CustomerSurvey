using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.DeactivateBranchAdmin
{
    internal sealed class DeactivateBranchAdminCommandValidator
        : AbstractValidator<DeactivateBranchAdminCommand>
    {
        public DeactivateBranchAdminCommandValidator()
        {
            RuleFor(x => x.BranchAdminId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeactivateBranchAdmin_BranchAdminId_Required);
        }
    }
}
