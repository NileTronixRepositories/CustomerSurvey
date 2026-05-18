using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.DeleteGlobalQuestion
{
    internal sealed class DeleteGlobalQuestionCommandValidator
        : AbstractValidator<DeleteGlobalQuestionCommand>
    {
        public DeleteGlobalQuestionCommandValidator()
        {
            RuleFor(x => x.QuestionId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeleteGlobalQuestion_QuestionId_Required);
        }
    }
}