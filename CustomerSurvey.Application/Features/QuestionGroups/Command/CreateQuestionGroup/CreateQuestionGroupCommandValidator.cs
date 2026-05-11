using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.CreateQuestionGroup
{
    internal sealed class CreateQuestionGroupCommandValidator
         : AbstractValidator<CreateQuestionGroupCommand>
    {
        public CreateQuestionGroupCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateQuestionGroup_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateQuestionGroup_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateQuestionGroup_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));
        }
    }
}