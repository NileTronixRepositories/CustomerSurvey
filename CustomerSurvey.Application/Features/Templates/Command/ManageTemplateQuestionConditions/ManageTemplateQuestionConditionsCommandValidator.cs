using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    internal sealed class ManageTemplateQuestionConditionsCommandValidator
         : AbstractValidator<ManageTemplateQuestionConditionsCommand>
    {
        public ManageTemplateQuestionConditionsCommandValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.ManageTemplateQuestionConditions_TemplateId_Required);

            RuleForEach(x => x.Conditions)
                .ChildRules(condition =>
                {
                    condition.RuleFor(x => x.ParentTemplateQuestionId)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.ManageTemplateQuestionConditions_ParentTemplateQuestionId_Required);

                    condition.RuleFor(x => x.ChildTemplateQuestionId)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.ManageTemplateQuestionConditions_ChildTemplateQuestionId_Required);

                    condition.RuleFor(x => x.TriggerType)
                        .IsInEnum()
                        .WithMessage(ErrorMessage.ManageTemplateQuestionConditions_TriggerType_Invalid);

                    condition.RuleFor(x => x.Order)
                        .GreaterThan(0)
                        .WithMessage(ErrorMessage.ManageTemplateQuestionConditions_Order_Invalid);
                });

            RuleFor(x => x.Conditions)
                .Must(NotContainDuplicateConditionKeys)
                .WithMessage(ErrorMessage.ManageTemplateQuestionConditions_Duplicated);
        }

        private static bool NotContainDuplicateConditionKeys(
            IReadOnlyCollection<TemplateQuestionConditionCommandItem> conditions)
        {
            if (conditions.Count == 0)
            {
                return true;
            }

            var keys = conditions
                .Select(x => new
                {
                    x.ParentTemplateQuestionId,
                    x.ChildTemplateQuestionId,
                    x.TriggerType,
                    x.SelectedQuestionOptionId,
                    x.TriggerValue
                })
                .ToArray();

            return keys.Length == keys.Distinct().Count();
        }
    }
}