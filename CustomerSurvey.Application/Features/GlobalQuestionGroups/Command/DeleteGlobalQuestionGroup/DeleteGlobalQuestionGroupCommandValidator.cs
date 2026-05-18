using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.DeleteGlobalQuestionGroup
{
    internal sealed class DeleteGlobalQuestionGroupCommandValidator
         : AbstractValidator<DeleteGlobalQuestionGroupCommand>
    {
        public DeleteGlobalQuestionGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeleteGlobalQuestionGroup_GroupId_Required);
        }
    }
}