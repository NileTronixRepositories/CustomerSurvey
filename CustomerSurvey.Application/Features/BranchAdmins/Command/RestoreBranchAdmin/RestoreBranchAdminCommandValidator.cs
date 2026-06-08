using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.RestoreBranchAdmin
{
    internal sealed class RestoreBranchAdminCommandValidator
        : AbstractValidator<RestoreBranchAdminCommand>
    {
        public RestoreBranchAdminCommandValidator()
        {
            RuleFor(x => x.BranchAdminId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreBranchAdmin_BranchAdminId_Required);
        }
    }
}
