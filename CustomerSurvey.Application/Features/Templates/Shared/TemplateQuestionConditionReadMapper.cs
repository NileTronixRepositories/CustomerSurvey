using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Shared
{
    internal static class TemplateQuestionConditionReadMapper
    {
        public static TemplateQuestionConditionResponse ToResponse(
            this TemplateQuestionConditionForReadDto condition)
        {
            return new TemplateQuestionConditionResponse
            {
                ConditionId = condition.ConditionId,
                ParentTemplateQuestionId = condition.ParentTemplateQuestionId,
                ChildTemplateQuestionId = condition.ChildTemplateQuestionId,

                TriggerType = (int)condition.TriggerType,
                TriggerTypeName = condition.TriggerType.ToString(),

                SelectedQuestionOptionId = condition.SelectedQuestionOptionId,
                TriggerValue = condition.TriggerValue,
                Order = condition.Order
            };
        }
    }
}