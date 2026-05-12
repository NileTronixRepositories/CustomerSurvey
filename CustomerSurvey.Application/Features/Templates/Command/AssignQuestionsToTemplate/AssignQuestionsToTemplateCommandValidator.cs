using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed class AssignQuestionsToTemplateCommandValidator
       : AbstractValidator<AssignQuestionsToTemplateCommand>
    {
        public AssignQuestionsToTemplateCommandValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignQuestionsToTemplate_TemplateId_Required);

            RuleFor(x => x.QuestionIds)
                .NotNull()
                .WithMessage(ErrorMessage.AssignQuestionsToTemplate_QuestionIds_Required)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignQuestionsToTemplate_QuestionIds_Required);

            RuleForEach(x => x.QuestionIds)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignQuestionsToTemplate_QuestionId_Required);

            RuleFor(x => x.QuestionIds)
                .Must(questionIds =>
                    questionIds is not null &&
                    questionIds.Distinct().Count() == questionIds.Count)
                .WithMessage(ErrorMessage.AssignQuestionsToTemplate_QuestionIds_Duplicated);
        }
    }
}