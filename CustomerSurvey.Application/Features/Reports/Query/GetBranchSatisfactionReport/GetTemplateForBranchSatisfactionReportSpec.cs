using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed class GetTemplateForBranchSatisfactionReportSpec
        : Specification<Template, TemplateForBranchSatisfactionReportDto>
    {
        public GetTemplateForBranchSatisfactionReportSpec(
            Guid templateId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == templateId &&
                x.BranchId == branchId);

            Select(x => new TemplateForBranchSatisfactionReportDto
            {
                TemplateId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}