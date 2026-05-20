using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate
{
    internal sealed class GetAnonymousTemplateCustomInputsForUpdateSpec
        : Specification<AnonymousTemplateCustomInput>
    {
        public GetAnonymousTemplateCustomInputsForUpdateSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId);
        }
    }
}