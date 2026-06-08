using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion
{
    internal sealed class UpdateGlobalQuestionCommandValidator
        : AbstractValidator<UpdateGlobalQuestionCommand>
    {
        public UpdateGlobalQuestionCommandValidator()
        {
            RuleFor(x => x.QuestionId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateGlobalQuestion_QuestionId_Required);

            RuleFor(x => x.GroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateGlobalQuestion_GroupId_Required);

            RuleFor(x => x.TextEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateGlobalQuestion_TextEn_Required)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.UpdateGlobalQuestion_TextEn_MaxLength);

            RuleFor(x => x.TextAr)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.UpdateGlobalQuestion_TextAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.TextAr));

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(ErrorMessage.UpdateGlobalQuestion_Type_Invalid);

            When(x => x.Type == QuestionType.SingleChoice, () =>
            {
                RuleFor(x => x.Options)
                    .NotEmpty()
                    .WithMessage(ErrorMessage.UpdateGlobalQuestion_Options_Required);

                RuleForEach(x => x.Options)
                    .ChildRules(option =>
                    {
                        option.RuleFor(x => x.TextEn)
                            .NotEmpty()
                            .WithMessage(ErrorMessage.UpdateGlobalQuestion_Option_TextEn_Required)
                            .MaximumLength(500)
                            .WithMessage(ErrorMessage.UpdateGlobalQuestion_Option_TextEn_MaxLength);

                        option.RuleFor(x => x.TextAr)
                            .MaximumLength(500)
                            .WithMessage(ErrorMessage.UpdateGlobalQuestion_Option_TextAr_MaxLength)
                            .When(x => !string.IsNullOrWhiteSpace(x.TextAr));

                        option.RuleFor(x => x.Order)
                            .GreaterThan(0)
                            .WithMessage(ErrorMessage.UpdateGlobalQuestion_Option_Order_Invalid);

                        option.RuleFor(x => x.Value)
                            .InclusiveBetween(1, 5)
                            .WithMessage(ErrorMessage.UpdateGlobalQuestion_Option_Value_Invalid);
                    });
            });

            When(x => x.Type != QuestionType.SingleChoice, () =>
            {
                RuleFor(x => x.Options)
                    .Must(x => x.Count == 0)
                    .WithMessage(command => command.Type == QuestionType.Image
                        ? ErrorMessage.Question_Image_Options_NotAllowed
                        : ErrorMessage.UpdateGlobalQuestion_Options_NotAllowed);
            });
        }
    }
}
