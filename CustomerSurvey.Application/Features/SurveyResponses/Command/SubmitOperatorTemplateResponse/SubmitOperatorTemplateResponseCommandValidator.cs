using CustomerSurvey.Domain.Resources;
using CustomerSurvey.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed class SubmitOperatorTemplateResponseCommandValidator
       : AbstractValidator<SubmitOperatorTemplateResponseCommand>
    {
        public SubmitOperatorTemplateResponseCommandValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.SubmitOperatorTemplateResponse_TemplateId_Required);

            RuleFor(x => x.Answers)
                .NotNull()
                .WithMessage(ErrorMessage.SubmitOperatorTemplateResponse_Answers_Required);

            RuleForEach(x => x.Flags).ChildRules(flag =>
            {
                flag.RuleFor(x => x.Type).IsInEnum();
                flag.RuleFor(x => x.Brief).MaximumLength(SurveyResponseFlag.MaxBriefLength)
                    .When(x => !string.IsNullOrWhiteSpace(x.Brief));
            });

            RuleForEach(x => x.Answers)
                .ChildRules(answer =>
                {
                    answer.RuleFor(x => x.QuestionId)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.SubmitOperatorTemplateResponse_QuestionId_Required);

                    answer.RuleFor(x => x.TextAnswer)
                        .MaximumLength(4000)
                        .WithMessage(ErrorMessage.SubmitOperatorTemplateResponse_TextAnswer_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.TextAnswer));
                });
        }
    }
}