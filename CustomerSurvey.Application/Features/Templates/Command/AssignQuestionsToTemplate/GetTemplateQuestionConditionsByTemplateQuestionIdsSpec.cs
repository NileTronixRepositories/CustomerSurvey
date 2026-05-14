using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed class GetTemplateQuestionConditionsByTemplateQuestionIdsSpec
         : Specification<TemplateQuestionCondition>
    {
        public GetTemplateQuestionConditionsByTemplateQuestionIdsSpec(
            IReadOnlyCollection<Guid> templateQuestionIds)
        {
            AddCriteria(x =>
                templateQuestionIds.Contains(x.ParentTemplateQuestionId) ||
                templateQuestionIds.Contains(x.ChildTemplateQuestionId));
        }
    }
}