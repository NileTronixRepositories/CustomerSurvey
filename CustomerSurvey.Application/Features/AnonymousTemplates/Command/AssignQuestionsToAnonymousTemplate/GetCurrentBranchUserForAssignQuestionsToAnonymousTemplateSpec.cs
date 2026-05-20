using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    internal sealed class GetCurrentBranchUserForAssignQuestionsToAnonymousTemplateSpec
        : Specification<BranchUser, CurrentBranchActorForAssignQuestionsToAnonymousTemplateDto>
    {
        public GetCurrentBranchUserForAssignQuestionsToAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForAssignQuestionsToAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}