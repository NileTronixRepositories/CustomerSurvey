using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    internal sealed class GetAnonymousTemplateConditionsRelatedToRemovedQuestionsSpec
        : Specification<AnonymousTemplateQuestionCondition>
    {
        public GetAnonymousTemplateConditionsRelatedToRemovedQuestionsSpec(
            Guid anonymousTemplateId,
            IReadOnlyCollection<Guid> removedAnonymousTemplateQuestionIds)
        {
            AddCriteria(x =>
                x.AnonymousTemplateId == anonymousTemplateId &&
                (
                    removedAnonymousTemplateQuestionIds.Contains(x.ParentAnonymousTemplateQuestionId) ||
                    removedAnonymousTemplateQuestionIds.Contains(x.ChildAnonymousTemplateQuestionId)
                ));
        }
    }
}