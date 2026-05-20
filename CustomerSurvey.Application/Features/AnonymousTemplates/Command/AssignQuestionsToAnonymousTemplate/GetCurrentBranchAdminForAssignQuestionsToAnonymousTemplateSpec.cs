using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    internal sealed class GetCurrentBranchAdminForAssignQuestionsToAnonymousTemplateSpec
        : Specification<BranchAdmin, CurrentBranchActorForAssignQuestionsToAnonymousTemplateDto>
    {
        public GetCurrentBranchAdminForAssignQuestionsToAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForAssignQuestionsToAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}