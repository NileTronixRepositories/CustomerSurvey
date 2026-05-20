using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed class ManageAnonymousTemplateQuestionConditionsCommandValidator
        : AbstractValidator<ManageAnonymousTemplateQuestionConditionsCommand>
    {
        public ManageAnonymousTemplateQuestionConditionsCommandValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_AnonymousTemplateId_Required);

            RuleFor(x => x.Conditions)
                .NotNull()
                .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_Conditions_Invalid);

            RuleForEach(x => x.Conditions)
                .ChildRules(condition =>
                {
                    condition.RuleFor(x => x.ParentAnonymousTemplateQuestionId)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_ParentQuestionId_Required);

                    condition.RuleFor(x => x.ChildAnonymousTemplateQuestionId)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_ChildQuestionId_Required);

                    condition.RuleFor(x => x)
                        .Must(x => x.ParentAnonymousTemplateQuestionId != x.ChildAnonymousTemplateQuestionId)
                        .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_ParentAndChild_CannotBeSame);

                    condition.RuleFor(x => x.TriggerType)
                        .IsInEnum()
                        .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_TriggerType_Invalid);

                    condition.RuleFor(x => x.Order)
                        .GreaterThan(0)
                        .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_Order_Invalid);

                    condition.RuleFor(x => x)
                        .Must(HaveValidTriggerShape)
                        .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_TriggerShape_Invalid);
                });

            RuleFor(x => x.Conditions)
                .Must(HaveUniqueOrders)
                .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_Order_Duplicated);

            RuleFor(x => x.Conditions)
                .Must(HaveUniqueLogicalConditions)
                .WithMessage(ErrorMessage.ManageAnonymousTemplateQuestionConditions_Duplicated);
        }

        private static bool HaveValidTriggerShape(
            ManageAnonymousTemplateQuestionConditionCommandItem condition)
        {
            return condition.TriggerType switch
            {
                QuestionConditionTriggerType.SingleChoiceOption =>
                    condition.SelectedQuestionOptionId.HasValue &&
                    condition.SelectedQuestionOptionId.Value != Guid.Empty &&
                    condition.TriggerValue is null,

                QuestionConditionTriggerType.StarRatingValue =>
                    !condition.SelectedQuestionOptionId.HasValue &&
                    condition.TriggerValue is >= 1 and <= 5,

                QuestionConditionTriggerType.SmileValue =>
                    !condition.SelectedQuestionOptionId.HasValue &&
                    condition.TriggerValue is >= 1 and <= 5,

                _ => false
            };
        }

        private static bool HaveUniqueOrders(
            IReadOnlyCollection<ManageAnonymousTemplateQuestionConditionCommandItem> conditions)
        {
            var orders = conditions
                .Where(x => x.Order > 0)
                .Select(x => x.Order)
                .ToArray();

            return orders.Length == orders.Distinct().Count();
        }

        private static bool HaveUniqueLogicalConditions(
            IReadOnlyCollection<ManageAnonymousTemplateQuestionConditionCommandItem> conditions)
        {
            var keys = conditions
                .Select(x => new
                {
                    x.ParentAnonymousTemplateQuestionId,
                    x.ChildAnonymousTemplateQuestionId,
                    x.TriggerType,
                    SelectedQuestionOptionId = x.SelectedQuestionOptionId ?? Guid.Empty,
                    TriggerValue = x.TriggerValue ?? 0
                })
                .ToArray();

            return keys.Length == keys.Distinct().Count();
        }
    }
}