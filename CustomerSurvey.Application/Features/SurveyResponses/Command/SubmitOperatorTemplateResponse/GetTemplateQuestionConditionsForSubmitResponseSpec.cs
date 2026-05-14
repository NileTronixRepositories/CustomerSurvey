using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed class GetTemplateQuestionConditionsForSubmitResponseSpec
        : Specification<TemplateQuestionCondition, TemplateQuestionConditionForSubmitResponseDto>
    {
        public GetTemplateQuestionConditionsForSubmitResponseSpec(Guid templateId)
        {
            AddCriteria(x => x.TemplateId == templateId);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateQuestionConditionForSubmitResponseDto
            {
                ConditionId = x.Id,
                TemplateId = x.TemplateId,
                ParentTemplateQuestionId = x.ParentTemplateQuestionId,
                ChildTemplateQuestionId = x.ChildTemplateQuestionId,
                TriggerType = x.TriggerType,
                SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                TriggerValue = x.TriggerValue,
                Order = x.Order
            });
        }
    }
}