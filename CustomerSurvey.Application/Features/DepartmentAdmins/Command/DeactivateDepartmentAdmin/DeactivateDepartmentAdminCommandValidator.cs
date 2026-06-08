using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.DeactivateDepartmentAdmin
{
    internal sealed class DeactivateDepartmentAdminCommandValidator
        : AbstractValidator<DeactivateDepartmentAdminCommand>
    {
        public DeactivateDepartmentAdminCommandValidator()
        {
            RuleFor(x => x.DepartmentAdminId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeactivateDepartmentAdmin_DepartmentAdminId_Required);
        }
    }
}
