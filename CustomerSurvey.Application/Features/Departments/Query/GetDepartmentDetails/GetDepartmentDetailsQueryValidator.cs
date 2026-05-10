using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentDetails
{
    internal sealed class GetDepartmentDetailsQueryValidator
         : AbstractValidator<GetDepartmentDetailsQuery>
    {
        public GetDepartmentDetailsQueryValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetDepartmentDetails_DepartmentId_Required);
        }
    }
}