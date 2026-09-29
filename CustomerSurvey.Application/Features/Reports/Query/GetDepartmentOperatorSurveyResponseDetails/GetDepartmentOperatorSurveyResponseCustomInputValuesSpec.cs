using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed class GetDepartmentOperatorSurveyResponseCustomInputValuesSpec
    : Specification<SurveyResponseCustomInputValue, DepartmentOperatorSurveyResponseCustomInputValueDto>
{
    public GetDepartmentOperatorSurveyResponseCustomInputValuesSpec(Guid surveyResponseId)
    {
        AddCriteria(x => x.SurveyResponseId == surveyResponseId);

        Select(x => new DepartmentOperatorSurveyResponseCustomInputValueDto
        {
            CustomInputId = x.TemplateCustomInputId,
            LabelEnSnapshot = x.LabelEnSnapshot,
            LabelArSnapshot = x.LabelArSnapshot,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue
        });
    }
}
