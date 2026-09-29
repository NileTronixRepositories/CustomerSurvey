using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

internal sealed class GetDepartmentOperatorSurveyResponseCustomInputPreviewsSpec
    : Specification<SurveyResponseCustomInputValue, DepartmentOperatorSurveyResponseCustomInputPreviewDto>
{
    public GetDepartmentOperatorSurveyResponseCustomInputPreviewsSpec(
        IReadOnlyCollection<Guid> surveyResponseIds)
    {
        AddCriteria(x => surveyResponseIds.Contains(x.SurveyResponseId));

        Select(x => new DepartmentOperatorSurveyResponseCustomInputPreviewDto
        {
            SurveyResponseId = x.SurveyResponseId,
            CustomInputId = x.TemplateCustomInputId,
            LabelEnSnapshot = x.LabelEnSnapshot,
            LabelArSnapshot = x.LabelArSnapshot,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue
        });
    }
}
