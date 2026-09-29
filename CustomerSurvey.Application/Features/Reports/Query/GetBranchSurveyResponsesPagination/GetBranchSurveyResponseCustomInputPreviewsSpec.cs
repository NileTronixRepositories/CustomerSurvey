using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;

internal sealed class GetBranchSurveyResponseCustomInputPreviewsSpec
    : Specification<SurveyResponseCustomInputValue, BranchSurveyResponseCustomInputPreviewDto>
{
    public GetBranchSurveyResponseCustomInputPreviewsSpec(
        IReadOnlyCollection<Guid> surveyResponseIds)
    {
        AddCriteria(x => surveyResponseIds.Contains(x.SurveyResponseId));

        Select(x => new BranchSurveyResponseCustomInputPreviewDto
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
