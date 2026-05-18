using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.RestoreGlobalQuestionGroup
{
    internal sealed class RestoreGlobalQuestionGroupCommandValidator
        : AbstractValidator<RestoreGlobalQuestionGroupCommand>
    {
        public RestoreGlobalQuestionGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreGlobalQuestionGroup_GroupId_Required);
        }
    }
}