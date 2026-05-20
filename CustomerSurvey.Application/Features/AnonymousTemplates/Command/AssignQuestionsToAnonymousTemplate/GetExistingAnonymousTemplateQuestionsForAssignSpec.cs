using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    internal sealed class GetExistingAnonymousTemplateQuestionsForAssignSpec
        : Specification<AnonymousTemplateQuestion>
    {
        public GetExistingAnonymousTemplateQuestionsForAssignSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId);
        }
    }
}