using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.RestoreGlobalQuestion
{
    internal sealed class RestoreGlobalQuestionCommandValidator
        : AbstractValidator<RestoreGlobalQuestionCommand>
    {
        public RestoreGlobalQuestionCommandValidator()
        {
            RuleFor(x => x.QuestionId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreGlobalQuestion_QuestionId_Required);
        }
    }
}