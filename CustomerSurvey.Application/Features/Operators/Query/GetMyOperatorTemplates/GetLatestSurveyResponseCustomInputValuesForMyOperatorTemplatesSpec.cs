using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed class GetLatestSurveyResponseCustomInputValuesForMyOperatorTemplatesSpec
        : Specification<SurveyResponseCustomInputValue, LatestSurveyResponseCustomInputValueForMyOperatorTemplateDto>
    {
        public GetLatestSurveyResponseCustomInputValuesForMyOperatorTemplatesSpec(
            IReadOnlyCollection<Guid> surveyResponseIds)
        {
            AddCriteria(x => surveyResponseIds.Contains(x.SurveyResponseId));

            Select(x => new LatestSurveyResponseCustomInputValueForMyOperatorTemplateDto
            {
                SurveyResponseId = x.SurveyResponseId,
                CustomInputId = x.TemplateCustomInputId,
                Name = x.NameSnapshot,
                Type = x.TypeSnapshot,
                StringValue = x.StringValue,
                IntegerValue = x.IntegerValue
            });
        }
    }
}