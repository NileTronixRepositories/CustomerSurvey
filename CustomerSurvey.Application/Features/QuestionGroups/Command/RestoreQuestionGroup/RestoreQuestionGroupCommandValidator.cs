using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup
{
    internal sealed class RestoreQuestionGroupCommandValidator
     : AbstractValidator<RestoreQuestionGroupCommand>
    {
        public RestoreQuestionGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreQuestionGroup_GroupId_Required);
        }
    }
}