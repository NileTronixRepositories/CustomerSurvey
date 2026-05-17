using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesForSelection
{
    internal sealed class GetTemplatesForSelectionSpec
      : Specification<Template, TemplateSelectionResponse>
    {
        public GetTemplatesForSelectionSpec(Guid? branchId)
        {
            AddCriteria(x => x.IsActive);

            if (branchId.HasValue)
            {
                AddCriteria(x => x.BranchId == branchId.Value);
            }

            AddOrderBy(x => x.NameEn);

            Select(x => new TemplateSelectionResponse
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                BranchId = x.BranchId,
                BranchNameEn = x.Branch.NameEn,
                BranchNameAr = x.Branch.NameAr,
                BranchCode = x.Branch.Code,
                ActiveFrom = x.ActiveFrom,
                ExpireTo = x.ExpireTo
            });
        }
    }
}