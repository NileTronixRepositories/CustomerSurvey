using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed class GetQuestionOptionsForDepartmentOperatorSurveyResponseDetailsSpec
    : Specification<QuestionOption, QuestionOptionForDepartmentOperatorSurveyResponseDetailsDto>
{
    public GetQuestionOptionsForDepartmentOperatorSurveyResponseDetailsSpec(
        IReadOnlyCollection<Guid> optionIds)
    {
        AddCriteria(x => optionIds.Contains(x.Id));

        Select(x => new QuestionOptionForDepartmentOperatorSurveyResponseDetailsDto
        {
            OptionId = x.Id,
            TextEn = x.TextEn,
            TextAr = x.TextAr,
            Value = x.Value
        });
    }
}
