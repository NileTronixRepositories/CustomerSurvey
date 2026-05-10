using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    internal sealed class GetBranchDetailsQueryValidator
        : AbstractValidator<GetBranchDetailsQuery>
    {
        public GetBranchDetailsQueryValidator()
        {
            RuleFor(x => x.BranchId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetBranchDetails_BranchId_Required);
        }
    }
}