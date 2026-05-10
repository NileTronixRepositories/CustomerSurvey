using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.UpdateDepartment
{
    internal sealed class UpdateDepartmentCommandValidator
         : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentCommandValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateDepartment_DepartmentId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateDepartment_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateDepartment_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateDepartment_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));
        }
    }
}