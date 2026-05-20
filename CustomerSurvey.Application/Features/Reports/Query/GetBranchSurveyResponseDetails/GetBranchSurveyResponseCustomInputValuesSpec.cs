using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed class GetBranchSurveyResponseCustomInputValuesSpec
    : Specification<SurveyResponseCustomInputValue, BranchSurveyResponseCustomInputValueDto>
{
    public GetBranchSurveyResponseCustomInputValuesSpec(Guid surveyResponseId)
    {
        AddCriteria(x => x.SurveyResponseId == surveyResponseId);

        Select(x => new BranchSurveyResponseCustomInputValueDto
        {
            CustomInputId = x.TemplateCustomInputId,
            NameSnapshot = x.NameSnapshot,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue
        });
    }
}