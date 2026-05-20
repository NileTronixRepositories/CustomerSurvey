using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed class SubmitAnonymousTemplateResponseCommandValidator
        : AbstractValidator<SubmitAnonymousTemplateResponseCommand>
    {
        public SubmitAnonymousTemplateResponseCommandValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.SubmitAnonymousTemplateResponse_AnonymousTemplateId_Required);

            RuleFor(x => x.CustomInputValues)
                .NotNull()
                .WithMessage(ErrorMessage.SubmitAnonymousTemplateResponse_CustomInputValues_Invalid);

            RuleFor(x => x.Answers)
                .NotNull()
                .WithMessage(ErrorMessage.SubmitAnonymousTemplateResponse_Answers_Invalid);

            RuleForEach(x => x.CustomInputValues)
                .ChildRules(input =>
                {
                    input.RuleFor(x => x.CustomInputId)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.SubmitAnonymousTemplateResponse_CustomInputId_Required);
                });

            RuleForEach(x => x.Answers)
                .ChildRules(answer =>
                {
                    answer.RuleFor(x => x.AnonymousTemplateQuestionId)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.SubmitAnonymousTemplateResponse_AnonymousTemplateQuestionId_Required);
                });

            RuleFor(x => x.CustomInputValues)
                .Must(HaveUniqueCustomInputIds)
                .WithMessage(ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_Duplicated);

            RuleFor(x => x.Answers)
                .Must(HaveUniqueQuestionIds)
                .WithMessage(ErrorMessage.SubmitAnonymousTemplateResponse_Answer_Duplicated);
        }

        private static bool HaveUniqueCustomInputIds(
            IReadOnlyCollection<SubmitAnonymousTemplateCustomInputValueCommandItem> values)
        {
            var ids = values
                .Where(x => x.CustomInputId != Guid.Empty)
                .Select(x => x.CustomInputId)
                .ToArray();

            return ids.Length == ids.Distinct().Count();
        }

        private static bool HaveUniqueQuestionIds(
            IReadOnlyCollection<SubmitAnonymousTemplateAnswerCommandItem> answers)
        {
            var ids = answers
                .Where(x => x.AnonymousTemplateQuestionId != Guid.Empty)
                .Select(x => x.AnonymousTemplateQuestionId)
                .ToArray();

            return ids.Length == ids.Distinct().Count();
        }
    }
}