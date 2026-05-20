using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

internal sealed class GetSystemSurveyResponseCustomInputValuesSpec
    : Specification<SurveyResponseCustomInputValue, SystemSurveyResponseCustomInputValueDto>
{
    public GetSystemSurveyResponseCustomInputValuesSpec(Guid surveyResponseId)
    {
        AddCriteria(x => x.SurveyResponseId == surveyResponseId);

        Select(x => new SystemSurveyResponseCustomInputValueDto
        {
            CustomInputId = x.TemplateCustomInputId,
            NameSnapshot = x.NameSnapshot,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue
        });
    }
}