using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.UpdateGlobalQuestionGroup
{
    internal sealed class UpdateGlobalQuestionGroupCommandValidator
          : AbstractValidator<UpdateGlobalQuestionGroupCommand>
    {
        public UpdateGlobalQuestionGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateGlobalQuestionGroup_GroupId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateGlobalQuestionGroup_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateGlobalQuestionGroup_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateGlobalQuestionGroup_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));
        }
    }
}