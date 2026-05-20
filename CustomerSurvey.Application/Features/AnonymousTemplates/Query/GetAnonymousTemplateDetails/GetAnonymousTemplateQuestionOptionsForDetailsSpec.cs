using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetAnonymousTemplateQuestionOptionsForDetailsSpec
        : Specification<QuestionOption, AnonymousTemplateDetailsQuestionOptionResponse>
    {
        public GetAnonymousTemplateQuestionOptionsForDetailsSpec(
            IReadOnlyCollection<Guid> questionIds)
        {
            AddCriteria(x =>
                questionIds.Contains(x.QuestionId) &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new AnonymousTemplateDetailsQuestionOptionResponse
            {
                OptionId = x.Id,
                QuestionId = x.QuestionId,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Order = x.Order,
                Value = x.Value,
                IsActive = x.IsActive
            });
        }
    }
}