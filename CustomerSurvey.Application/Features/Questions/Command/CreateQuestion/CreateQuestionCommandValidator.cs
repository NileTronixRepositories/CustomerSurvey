using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.CreateQuestion
{
    internal sealed class CreateQuestionCommandValidator
       : AbstractValidator<CreateQuestionCommand>
    {
        public CreateQuestionCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateQuestion_GroupId_Required);

            RuleFor(x => x.TextEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateQuestion_TextEn_Required)
                .MaximumLength(500)
                .WithMessage(ErrorMessage.CreateQuestion_TextEn_MaxLength);

            RuleFor(x => x.TextAr)
                .MaximumLength(500)
                .WithMessage(ErrorMessage.CreateQuestion_TextAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.TextAr));

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(ErrorMessage.CreateQuestion_Type_Invalid);
        }
    }
}