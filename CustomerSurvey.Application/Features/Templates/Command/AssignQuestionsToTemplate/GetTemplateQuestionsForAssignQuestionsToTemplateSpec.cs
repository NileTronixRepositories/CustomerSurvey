using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed class GetTemplateQuestionsForAssignQuestionsToTemplateSpec
        : Specification<TemplateQuestion>
    {
        public GetTemplateQuestionsForAssignQuestionsToTemplateSpec(Guid templateId)
        {
            AddCriteria(x => x.TemplateId == templateId);
        }
    }
}