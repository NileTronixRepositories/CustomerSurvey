using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Shared
{
    internal sealed class GetTemplateQuestionConditionsByTemplateIdsSpec
         : Specification<TemplateQuestionCondition, TemplateQuestionConditionForReadDto>
    {
        public GetTemplateQuestionConditionsByTemplateIdsSpec(
            IReadOnlyCollection<Guid> templateIds)
        {
            AddCriteria(x => templateIds.Contains(x.TemplateId));

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