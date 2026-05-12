using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed class GetTemplateQuestionsSelectionQueryValidator
           : AbstractValidator<GetTemplateQuestionsSelectionQuery>
    {
        public GetTemplateQuestionsSelectionQueryValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetTemplateQuestionsSelection_TemplateId_Required);
        }
    }
}