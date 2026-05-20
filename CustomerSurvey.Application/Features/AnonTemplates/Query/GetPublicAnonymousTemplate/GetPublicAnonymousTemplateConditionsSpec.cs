using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate
{
    internal sealed class GetPublicAnonymousTemplateConditionsSpec
        : Specification<AnonymousTemplateQuestionCondition, PublicAnonymousTemplateQuestionConditionResponse>
    {
        public GetPublicAnonymousTemplateConditionsSpec(
            Guid anonymousTemplateId,
            IReadOnlyCollection<Guid> activeAnonymousTemplateQuestionIds)
        {
            AddCriteria(x =>
                x.AnonymousTemplateId == anonymousTemplateId &&
                x.IsActive &&
                activeAnonymousTemplateQuestionIds.Contains(x.ParentAnonymousTemplateQuestionId) &&
                activeAnonymousTemplateQuestionIds.Contains(x.ChildAnonymousTemplateQuestionId));

            AddOrderBy(x => x.Order);

            Select(x => new PublicAnonymousTemplateQuestionConditionResponse
            {
                ConditionId = x.Id,
                ParentAnonymousTemplateQuestionId = x.ParentAnonymousTemplateQuestionId,
                ChildAnonymousTemplateQuestionId = x.ChildAnonymousTemplateQuestionId,
                TriggerType = x.TriggerType,
                TriggerTypeName = x.TriggerType.ToString(),
                SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                TriggerValue = x.TriggerValue,
                Order = x.Order
            });
        }
    }
}