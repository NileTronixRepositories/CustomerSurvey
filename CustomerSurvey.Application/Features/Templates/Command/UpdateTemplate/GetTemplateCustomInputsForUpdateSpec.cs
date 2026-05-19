using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate
{
    internal sealed class GetTemplateCustomInputsForUpdateSpec
        : Specification<TemplateCustomInput>
    {
        public GetTemplateCustomInputsForUpdateSpec(Guid templateId)
        {
            AddCriteria(x => x.TemplateId == templateId);

            AddOrderBy(x => x.Order);
        }
    }
}