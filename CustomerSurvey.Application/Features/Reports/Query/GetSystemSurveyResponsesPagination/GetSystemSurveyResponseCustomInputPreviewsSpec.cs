using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;

internal sealed class GetSystemSurveyResponseCustomInputPreviewsSpec
    : Specification<SurveyResponseCustomInputValue, SystemSurveyResponseCustomInputPreviewDto>
{
    public GetSystemSurveyResponseCustomInputPreviewsSpec(
        IReadOnlyCollection<Guid> surveyResponseIds)
    {
        AddCriteria(x => surveyResponseIds.Contains(x.SurveyResponseId));

        Select(x => new SystemSurveyResponseCustomInputPreviewDto
        {
            SurveyResponseId = x.SurveyResponseId,
            NameSnapshot = x.NameSnapshot,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue
        });
    }
}