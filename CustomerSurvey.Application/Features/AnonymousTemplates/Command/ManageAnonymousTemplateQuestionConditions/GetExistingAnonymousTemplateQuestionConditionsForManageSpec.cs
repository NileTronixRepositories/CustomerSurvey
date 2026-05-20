using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed class GetExistingAnonymousTemplateQuestionConditionsForManageSpec
        : Specification<AnonymousTemplateQuestionCondition>
    {
        public GetExistingAnonymousTemplateQuestionConditionsForManageSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId);
        }
    }
}