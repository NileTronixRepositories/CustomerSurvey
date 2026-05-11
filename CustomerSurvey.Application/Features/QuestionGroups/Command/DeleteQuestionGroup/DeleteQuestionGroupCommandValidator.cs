using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.DeleteQuestionGroup
{
    internal sealed class DeleteQuestionGroupCommandValidator
        : AbstractValidator<DeleteQuestionGroupCommand>
    {
        public DeleteQuestionGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeleteQuestionGroup_GroupId_Required);
        }
    }
}