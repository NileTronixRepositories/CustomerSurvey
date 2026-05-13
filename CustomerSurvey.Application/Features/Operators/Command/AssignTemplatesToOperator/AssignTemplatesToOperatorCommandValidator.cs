using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator
{
    internal sealed class AssignTemplatesToOperatorCommandValidator
      : AbstractValidator<AssignTemplatesToOperatorCommand>
    {
        public AssignTemplatesToOperatorCommandValidator()
        {
            RuleFor(x => x.OperatorId)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignTemplatesToOperator_OperatorId_Required);

            RuleFor(x => x.TemplateIds)
                .NotNull()
                .WithMessage(ErrorMessage.AssignTemplatesToOperator_TemplateIds_Required);

            RuleForEach(x => x.TemplateIds)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignTemplatesToOperator_TemplateId_Invalid)
                .When(x => x.TemplateIds is not null);

            RuleFor(x => x.TemplateIds)
                .Must(templateIds =>
                    templateIds is not null &&
                    templateIds.Count == templateIds.Distinct().Count())
                .WithMessage(ErrorMessage.AssignTemplatesToOperator_TemplateIds_Duplicated)
                .When(x => x.TemplateIds is not null);
        }
    }
}