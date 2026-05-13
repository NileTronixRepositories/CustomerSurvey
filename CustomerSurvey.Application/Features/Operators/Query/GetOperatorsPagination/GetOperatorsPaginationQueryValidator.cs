using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorsPagination
{
    internal sealed class GetOperatorsPaginationQueryValidator
       : AbstractValidator<GetOperatorsPaginationQuery>
    {
        public GetOperatorsPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetOperatorsPagination_PageNumber_Invalid);

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetOperatorsPagination_PageSize_Invalid)
                .LessThanOrEqualTo(100)
                .WithMessage(ErrorMessage.GetOperatorsPagination_PageSize_Max);

            RuleFor(x => x.DepartmentId)
                .Must(x => !x.HasValue || x.Value != Guid.Empty)
                .WithMessage(ErrorMessage.GetOperatorsPagination_DepartmentId_Invalid);
        }
    }
}