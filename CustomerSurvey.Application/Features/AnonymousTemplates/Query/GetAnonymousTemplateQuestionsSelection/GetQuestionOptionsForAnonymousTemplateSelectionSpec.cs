using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed class GetQuestionOptionsForAnonymousTemplateSelectionSpec
        : Specification<QuestionOption, AnonymousTemplateQuestionSelectionOptionResponse>
    {
        public GetQuestionOptionsForAnonymousTemplateSelectionSpec(
            IReadOnlyCollection<Guid> questionIds)
        {
            AddCriteria(x =>
                questionIds.Contains(x.QuestionId) &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new AnonymousTemplateQuestionSelectionOptionResponse
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