using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    internal sealed class RestoreQuestionCommandValidator
           : AbstractValidator<RestoreQuestionCommand>
    {
        public RestoreQuestionCommandValidator()
        {
            RuleFor(x => x.QuestionId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreQuestion_QuestionId_Required);
        }
    }
}