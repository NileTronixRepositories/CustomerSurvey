using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetQuestionOptionsForResponseDetailsSpec
        : Specification<QuestionOption, QuestionOptionForResponseDetailsDto>
    {
        public GetQuestionOptionsForResponseDetailsSpec(
            IReadOnlyCollection<Guid> optionIds)
        {
            AddCriteria(x => optionIds.Contains(x.Id));

            Select(x => new QuestionOptionForResponseDetailsDto
            {
                OptionId = x.Id,
                QuestionId = x.QuestionId,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Value = x.Value
            });
        }
    }
}