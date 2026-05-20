using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed class GetAnonymousTemplateConditionsForSubmitSpec
        : Specification<AnonymousTemplateQuestionCondition, AnonymousTemplateConditionForSubmitDto>
    {
        public GetAnonymousTemplateConditionsForSubmitSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x =>
                x.AnonymousTemplateId == anonymousTemplateId &&
                x.IsActive);

            Select(x => new AnonymousTemplateConditionForSubmitDto
            {
                ConditionId = x.Id,
                ParentAnonymousTemplateQuestionId = x.ParentAnonymousTemplateQuestionId,
                ChildAnonymousTemplateQuestionId = x.ChildAnonymousTemplateQuestionId,
                TriggerType = x.TriggerType,
                SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                TriggerValue = x.TriggerValue,
                Order = x.Order,
                IsActive = x.IsActive
            });
        }
    }
}