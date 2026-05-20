using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed class GetQuestionOptionsForSurveyResponseDetailsSpec
    : Specification<QuestionOption, QuestionOptionForSurveyResponseDetailsDto>
{
    public GetQuestionOptionsForSurveyResponseDetailsSpec(
        IReadOnlyCollection<Guid> optionIds)
    {
        AddCriteria(x => optionIds.Contains(x.Id));

        Select(x => new QuestionOptionForSurveyResponseDetailsDto
        {
            OptionId = x.Id,
            TextEn = x.TextEn,
            TextAr = x.TextAr,
            Value = x.Value
        });
    }
}