using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed class GetSatisfactionSurveyResponsesForBranchReportSpec
     : Specification<SurveyResponse, SatisfactionSurveyResponseFlatDto>
    {
        public GetSatisfactionSurveyResponsesForBranchReportSpec(
            Guid branchId,
            DateTime fromUtc,
            DateTime toExclusiveUtc,
            Guid? templateId)
        {
            AddCriteria(x =>
                x.Template.BranchId == branchId &&
                x.SubmittedOnUtc >= fromUtc &&
                x.SubmittedOnUtc < toExclusiveUtc);

            if (templateId.HasValue)
            {
                AddCriteria(x => x.TemplateId == templateId.Value);
            }

            Select(x => new SatisfactionSurveyResponseFlatDto
            {
                SurveyResponseId = x.Id,
                TemplateId = x.TemplateId,
                TemplateNameEn = x.Template.NameEn,
                TemplateNameAr = x.Template.NameAr,
                SubmittedOnUtc = x.SubmittedOnUtc,
                ActualScore = x.ActualScore,
                MaxScore = x.MaxScore,
                ScorePercentage = x.ScorePercentage
            });
        }
    }
}