using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.RestoreAnonymousTemplate
{
    internal sealed class GetCurrentBranchUserForRestoreAnonymousTemplateSpec
        : Specification<BranchUser, CurrentBranchActorForRestoreAnonymousTemplateDto>
    {
        public GetCurrentBranchUserForRestoreAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForRestoreAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}