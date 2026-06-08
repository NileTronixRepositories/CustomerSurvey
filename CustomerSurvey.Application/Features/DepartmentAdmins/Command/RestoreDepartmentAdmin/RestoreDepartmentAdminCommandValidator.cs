using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.RestoreDepartmentAdmin
{
    internal sealed class RestoreDepartmentAdminCommandValidator
        : AbstractValidator<RestoreDepartmentAdminCommand>
    {
        public RestoreDepartmentAdminCommandValidator()
        {
            RuleFor(x => x.DepartmentAdminId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreDepartmentAdmin_DepartmentAdminId_Required);
        }
    }
}
