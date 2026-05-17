using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.BackgroundJobs.Templates
{
    internal sealed class GetExpiredActiveTemplatesSpec : Specification<Template>
    {
        public GetExpiredActiveTemplatesSpec(DateTime utcNow)
        {
            AddCriteria(x =>
                x.IsActive &&
                x.ExpireTo.HasValue &&
                x.ExpireTo.Value <= utcNow);

            AddOrderBy(x => x.ExpireTo);
        }
    }
}