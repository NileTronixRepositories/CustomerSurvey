using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    internal sealed class GetTemplateDetailsQueryValidator
        : AbstractValidator<GetTemplateDetailsQuery>
    {
        public GetTemplateDetailsQueryValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetTemplateDetails_TemplateId_Required);
        }
    }
}