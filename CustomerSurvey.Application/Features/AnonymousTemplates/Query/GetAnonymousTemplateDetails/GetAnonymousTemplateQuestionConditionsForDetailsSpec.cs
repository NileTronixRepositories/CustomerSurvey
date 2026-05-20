using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetAnonymousTemplateQuestionConditionsForDetailsSpec
        : Specification<AnonymousTemplateQuestionCondition, AnonymousTemplateDetailsQuestionConditionResponse>
    {
        public GetAnonymousTemplateQuestionConditionsForDetailsSpec(
            Guid anonymousTemplateId,
            IReadOnlyCollection<Guid> selectedAnonymousTemplateQuestionIds)
        {
            AddCriteria(x =>
                x.AnonymousTemplateId == anonymousTemplateId &&
                x.IsActive &&
                selectedAnonymousTemplateQuestionIds.Contains(x.ParentAnonymousTemplateQuestionId) &&
                selectedAnonymousTemplateQuestionIds.Contains(x.ChildAnonymousTemplateQuestionId));

            AddOrderBy(x => x.Order);

            Select(x => new AnonymousTemplateDetailsQuestionConditionResponse
            {
                ConditionId = x.Id,
                ParentAnonymousTemplateQuestionId = x.ParentAnonymousTemplateQuestionId,
                ChildAnonymousTemplateQuestionId = x.ChildAnonymousTemplateQuestionId,
                TriggerType = x.TriggerType,
                TriggerTypeName = x.TriggerType.ToString(),
                SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                TriggerValue = x.TriggerValue,
                Order = x.Order,
                IsActive = x.IsActive
            });
        }
    }
}