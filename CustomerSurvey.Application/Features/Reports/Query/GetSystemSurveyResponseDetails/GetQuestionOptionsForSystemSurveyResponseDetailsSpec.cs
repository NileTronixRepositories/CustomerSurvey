using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

internal sealed class GetQuestionOptionsForSystemSurveyResponseDetailsSpec
    : Specification<QuestionOption, QuestionOptionForSystemSurveyResponseDetailsDto>
{
    public GetQuestionOptionsForSystemSurveyResponseDetailsSpec(
        IReadOnlyCollection<Guid> optionIds)
    {
        AddCriteria(x => optionIds.Contains(x.Id));

        Select(x => new QuestionOptionForSystemSurveyResponseDetailsDto
        {
            OptionId = x.Id,
            TextEn = x.TextEn,
            TextAr = x.TextAr,
            Value = x.Value
        });
    }
}