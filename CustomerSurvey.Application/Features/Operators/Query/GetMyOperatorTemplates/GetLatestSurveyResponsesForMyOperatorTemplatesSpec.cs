using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed class GetLatestSurveyResponsesForMyOperatorTemplatesSpec
        : Specification<SurveyResponse, LatestSurveyResponseForMyOperatorTemplateDto>
    {
        public GetLatestSurveyResponsesForMyOperatorTemplatesSpec(
            Guid operatorId,
            IReadOnlyCollection<Guid> templateIds)
        {
            AddCriteria(x =>
                x.OperatorId == operatorId &&
                templateIds.Contains(x.TemplateId));

            AddOrderByDescending(x => x.SubmittedOnUtc);

            Select(x => new LatestSurveyResponseForMyOperatorTemplateDto
            {
                SurveyResponseId = x.Id,
                TemplateId = x.TemplateId,
                SubmittedOnUtc = x.SubmittedOnUtc,
                ActualScore = x.ActualScore,
                MaxScore = x.MaxScore,
                ScorePercentage = x.ScorePercentage
            });
        }
    }
}