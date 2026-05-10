using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.DeleteDepartment
{
    internal sealed class DeleteDepartmentCommandValidator
        : AbstractValidator<DeleteDepartmentCommand>
    {
        public DeleteDepartmentCommandValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeleteDepartment_DepartmentId_Required);
        }
    }
}