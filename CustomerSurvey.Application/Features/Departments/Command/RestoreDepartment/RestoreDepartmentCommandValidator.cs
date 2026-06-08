using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Departments.Command.RestoreDepartment
{
    internal sealed class RestoreDepartmentCommandValidator
        : AbstractValidator<RestoreDepartmentCommand>
    {
        public RestoreDepartmentCommandValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreDepartment_DepartmentId_Required);
        }
    }
}
