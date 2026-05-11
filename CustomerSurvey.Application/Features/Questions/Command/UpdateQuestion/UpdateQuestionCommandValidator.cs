using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion
{
    internal sealed class UpdateQuestionCommandValidator
         : AbstractValidator<UpdateQuestionCommand>
    {
        public UpdateQuestionCommandValidator()
        {
            RuleFor(x => x.QuestionId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateQuestion_QuestionId_Required);

            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateQuestion_GroupId_Required);

            RuleFor(x => x.TextEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateQuestion_TextEn_Required)
                .MaximumLength(500)
                .WithMessage(ErrorMessage.UpdateQuestion_TextEn_MaxLength);

            RuleFor(x => x.TextAr)
                .MaximumLength(500)
                .WithMessage(ErrorMessage.UpdateQuestion_TextAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.TextAr));

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(ErrorMessage.UpdateQuestion_Type_Invalid);
        }
    }
}