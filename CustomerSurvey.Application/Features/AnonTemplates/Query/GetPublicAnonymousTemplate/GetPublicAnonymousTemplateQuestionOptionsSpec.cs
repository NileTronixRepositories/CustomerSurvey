using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate
{
    internal sealed class GetPublicAnonymousTemplateQuestionOptionsSpec
        : Specification<QuestionOption, PublicAnonymousTemplateQuestionOptionResponse>
    {
        public GetPublicAnonymousTemplateQuestionOptionsSpec(
            IReadOnlyCollection<Guid> questionIds)
        {
            AddCriteria(x =>
                questionIds.Contains(x.QuestionId) &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new PublicAnonymousTemplateQuestionOptionResponse
            {
                OptionId = x.Id,
                QuestionId = x.QuestionId,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Order = x.Order,
                Value = x.Value
            });
        }
    }
}