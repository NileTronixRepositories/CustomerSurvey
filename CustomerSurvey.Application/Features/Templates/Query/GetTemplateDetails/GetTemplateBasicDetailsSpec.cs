using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    internal sealed class GetTemplateBasicDetailsSpec
         : Specification<Template, TemplateBasicDetailsDto>
    {
        public GetTemplateBasicDetailsSpec(
            Guid templateId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == templateId &&
                x.BranchId == branchId);

            Select(x => new TemplateBasicDetailsDto
            {
                TemplateId = x.Id,
                BranchId = x.BranchId,
                BranchNameEn = x.Branch.NameEn,
                BranchNameAr = x.Branch.NameAr,
                BranchCode = x.Branch.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,
                IsActive = x.IsActive,
                LogoPath = x.LogoPath,
                CreatedOnUtc = x.CreatedOnUtc,
                ModifiedOnUtc = x.ModifiedOnUtc,
                ActiveFrom = x.ActiveFrom,
                ExpireTo = x.ExpireTo,
            });
        }
    }
}
