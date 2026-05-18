using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.CreateGlobalQuestion
{
    internal sealed class CreateGlobalQuestionCommandValidator
         : AbstractValidator<CreateGlobalQuestionCommand>
    {
        public CreateGlobalQuestionCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateGlobalQuestion_GroupId_Required);

            RuleFor(x => x.TextEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateGlobalQuestion_TextEn_Required)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.CreateGlobalQuestion_TextEn_MaxLength);

            RuleFor(x => x.TextAr)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.CreateGlobalQuestion_TextAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.TextAr));

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(ErrorMessage.CreateGlobalQuestion_Type_Invalid);

            When(x => x.Type == QuestionType.SingleChoice, () =>
            {
                RuleFor(x => x.Options)
                    .NotEmpty()
                    .WithMessage(ErrorMessage.CreateGlobalQuestion_Options_Required);

                RuleForEach(x => x.Options)
                    .ChildRules(option =>
                    {
                        option.RuleFor(x => x.TextEn)
                            .NotEmpty()
                            .WithMessage(ErrorMessage.CreateGlobalQuestion_Option_TextEn_Required)
                            .MaximumLength(500)
                            .WithMessage(ErrorMessage.CreateGlobalQuestion_Option_TextEn_MaxLength);

                        option.RuleFor(x => x.TextAr)
                            .MaximumLength(500)
                            .WithMessage(ErrorMessage.CreateGlobalQuestion_Option_TextAr_MaxLength)
                            .When(x => !string.IsNullOrWhiteSpace(x.TextAr));

                        option.RuleFor(x => x.Order)
                            .GreaterThan(0)
                            .WithMessage(ErrorMessage.CreateGlobalQuestion_Option_Order_Invalid);

                        option.RuleFor(x => x.Value)
                            .InclusiveBetween(1, 5)
                            .WithMessage(ErrorMessage.CreateGlobalQuestion_Option_Value_Invalid);
                    });
            });

            When(x => x.Type != QuestionType.SingleChoice, () =>
            {
                RuleFor(x => x.Options)
                    .Must(x => x.Count == 0)
                    .WithMessage(ErrorMessage.CreateGlobalQuestion_Options_NotAllowed);
            });
        }
    }
}