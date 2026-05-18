using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion
{
    internal sealed class GetActiveTemplateQuestionConditionsByOptionIdsForUpdateGlobalQuestionSpec
           : Specification<TemplateQuestionCondition>
    {
        public GetActiveTemplateQuestionConditionsByOptionIdsForUpdateGlobalQuestionSpec(
            IReadOnlyCollection<Guid> optionIds)
        {
            AddCriteria(x =>
                x.IsActive &&
                x.SelectedQuestionOptionId.HasValue &&
                optionIds.Contains(x.SelectedQuestionOptionId.GetValueOrDefault()));
        }
    }
}