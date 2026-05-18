using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Templates.Shared;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed class GetTemplateQuestionConditionsForTemplateQuestionsSelectionSpec
       : Specification<TemplateQuestionCondition, TemplateQuestionConditionForReadDto>
    {
        public GetTemplateQuestionConditionsForTemplateQuestionsSelectionSpec(Guid templateId)
        {
            AddCriteria(x =>
                x.TemplateId == templateId &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateQuestionConditionForReadDto
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