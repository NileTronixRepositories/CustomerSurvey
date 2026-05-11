using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.UpdateQuestionGroup
{
    internal sealed class UpdateQuestionGroupCommandValidator
        : AbstractValidator<UpdateQuestionGroupCommand>
    {
        public UpdateQuestionGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateQuestionGroup_GroupId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateQuestionGroup_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateQuestionGroup_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateQuestionGroup_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));
        }
    }
}