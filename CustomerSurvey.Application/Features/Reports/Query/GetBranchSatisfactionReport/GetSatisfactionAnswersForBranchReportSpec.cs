using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed class GetSatisfactionAnswersForBranchReportSpec
         : Specification<SurveyAnswer, SatisfactionAnswerFlatDto>
    {
        public GetSatisfactionAnswersForBranchReportSpec(
            Guid branchId,
            DateTime fromUtc,
            DateTime toExclusiveUtc,
            Guid? templateId)
        {
            AddCriteria(x =>
                x.SurveyResponse.Template.BranchId == branchId &&
                x.SurveyResponse.SubmittedOnUtc >= fromUtc &&
                x.SurveyResponse.SubmittedOnUtc < toExclusiveUtc);

            if (templateId.HasValue)
            {
                AddCriteria(x => x.SurveyResponse.TemplateId == templateId.Value);
            }

            Select(x => new SatisfactionAnswerFlatDto
            {
                SurveyResponseId = x.SurveyResponseId,
                TemplateId = x.SurveyResponse.TemplateId,
                TemplateNameEn = x.SurveyResponse.Template.NameEn,
                TemplateNameAr = x.SurveyResponse.Template.NameAr,
                SubmittedOnUtc = x.SurveyResponse.SubmittedOnUtc,
                QuestionType = x.QuestionType,
                StarRatingValue = x.StarRatingValue,
                SmileValue = x.SmileValue,
                TextAnswer = x.TextAnswer,
                VoiceFileName = x.VoiceFileName
            });
        }
    }
}