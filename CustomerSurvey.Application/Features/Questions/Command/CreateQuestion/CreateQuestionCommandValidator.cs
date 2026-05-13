using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;

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

            RuleFor(x => x.Options)
                .Must((command, options) => IsValidOptionsShape(command.Type, options))
                .WithMessage(ErrorMessage.CreateQuestion_Options_InvalidForType);

            RuleFor(x => x.Options)
                .Must(options => options is not null && options.Count >= 2)
                .WithMessage(ErrorMessage.CreateQuestion_Options_MinCount)
                .When(x => x.Type == QuestionType.SingleChoice);

            RuleForEach(x => x.Options)
                .ChildRules(option =>
                {
                    option.RuleFor(x => x.TextEn)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.CreateQuestion_Option_TextEn_Required)
                        .MaximumLength(300)
                        .WithMessage(ErrorMessage.CreateQuestion_Option_TextEn_MaxLength);

                    option.RuleFor(x => x.TextAr)
                        .MaximumLength(300)
                        .WithMessage(ErrorMessage.CreateQuestion_Option_TextAr_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.TextAr));

                    option.RuleFor(x => x.Order)
                        .GreaterThan(0)
                        .WithMessage(ErrorMessage.CreateQuestion_Option_Order_Invalid);
                });

            RuleFor(x => x.Options)
                .Must(HaveUniqueTextEn)
                .WithMessage(ErrorMessage.CreateQuestion_Options_TextEn_Duplicated)
                .When(x => x.Type == QuestionType.SingleChoice);

            RuleFor(x => x.Options)
                .Must(HaveUniqueOrder)
                .WithMessage(ErrorMessage.CreateQuestion_Options_Order_Duplicated)
                .When(x => x.Type == QuestionType.SingleChoice);
        }

        private static bool IsValidOptionsShape(
            QuestionType type,
            IReadOnlyCollection<Shared.QuestionOptionCommandItem>? options)
        {
            var count = options?.Count ?? 0;

            return type == QuestionType.SingleChoice
                ? count >= 2
                : count == 0;
        }

        private static bool HaveUniqueTextEn(
            IReadOnlyCollection<Shared.QuestionOptionCommandItem>? options)
        {
            if (options is null)
            {
                return true;
            }

            return options
                .Where(x => !string.IsNullOrWhiteSpace(x.TextEn))
                .Select(x => x.TextEn.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() == options.Count;
        }

        private static bool HaveUniqueOrder(
            IReadOnlyCollection<Shared.QuestionOptionCommandItem>? options)
        {
            if (options is null)
            {
                return true;
            }

            return options
                .Select(x => x.Order)
                .Distinct()
                .Count() == options.Count;
        }
    }
}