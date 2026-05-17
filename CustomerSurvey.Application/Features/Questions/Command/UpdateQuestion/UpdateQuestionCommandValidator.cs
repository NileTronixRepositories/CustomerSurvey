using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;

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

            RuleFor(x => x.Options)
                .Must((command, options) => IsValidOptionsShape(command.Type, options))
                .WithMessage(ErrorMessage.UpdateQuestion_Options_InvalidForType);

            RuleFor(x => x.Options)
                .Must(options => options is not null && options.Count >= 2)
                .WithMessage(ErrorMessage.UpdateQuestion_Options_MinCount)
                .When(x => x.Type == QuestionType.SingleChoice);

            RuleForEach(x => x.Options)
      .ChildRules(option =>
      {
          option.RuleFor(x => x.TextEn)
              .NotEmpty()
              .WithMessage(ErrorMessage.UpdateQuestion_Option_TextEn_Required)
              .MaximumLength(300)
              .WithMessage(ErrorMessage.UpdateQuestion_Option_TextEn_MaxLength);

          option.RuleFor(x => x.TextAr)
              .MaximumLength(300)
              .WithMessage(ErrorMessage.UpdateQuestion_Option_TextAr_MaxLength)
              .When(x => !string.IsNullOrWhiteSpace(x.TextAr));

          option.RuleFor(x => x.Order)
              .GreaterThan(0)
              .WithMessage(ErrorMessage.UpdateQuestion_Option_Order_Invalid);

          option.RuleFor(x => x.Value)
              .InclusiveBetween(1, 5)
              .WithMessage(ErrorMessage.UpdateQuestion_Option_Value_Invalid);
      })
      .When(x => x.Type == QuestionType.SingleChoice);

            RuleFor(x => x.Options)
                .Must(HaveUniqueTextEn)
                .WithMessage(ErrorMessage.UpdateQuestion_Options_TextEn_Duplicated)
                .When(x => x.Type == QuestionType.SingleChoice);

            RuleFor(x => x.Options)
                .Must(HaveUniqueOrder)
                .WithMessage(ErrorMessage.UpdateQuestion_Options_Order_Duplicated)
                .When(x => x.Type == QuestionType.SingleChoice);

            RuleFor(x => x.Options)
    .Must(options =>
    {
        if (options is null)
        {
            return true;
        }

        var optionIds = options
            .Where(x => x.OptionId.HasValue)
            .Select(x => x.OptionId!.Value)
            .ToArray();

        return optionIds.Length == optionIds.Distinct().Count();
    })
    .WithMessage(ErrorMessage.UpdateQuestion_OptionId_Duplicated)
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