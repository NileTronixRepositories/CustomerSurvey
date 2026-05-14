using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    internal sealed class GetExistingTemplateQuestionConditionsSpec
          : Specification<TemplateQuestionCondition>
    {
        public GetExistingTemplateQuestionConditionsSpec(Guid templateId)
        {
            AddCriteria(x => x.TemplateId == templateId);
        }
    }
}