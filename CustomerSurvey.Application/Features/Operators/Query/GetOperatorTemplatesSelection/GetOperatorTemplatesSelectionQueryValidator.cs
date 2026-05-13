using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    internal sealed class GetOperatorTemplatesSelectionQueryValidator
         : AbstractValidator<GetOperatorTemplatesSelectionQuery>
    {
        public GetOperatorTemplatesSelectionQueryValidator()
        {
            RuleFor(x => x.OperatorId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetOperatorTemplatesSelection_OperatorId_Required);

            RuleFor(x => x.SearchText)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.GetOperatorTemplatesSelection_SearchText_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
        }
    }
}