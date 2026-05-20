using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.RestoreAnonymousTemplate
{
    internal sealed class GetCurrentBranchAdminForRestoreAnonymousTemplateSpec
        : Specification<BranchAdmin, CurrentBranchActorForRestoreAnonymousTemplateDto>
    {
        public GetCurrentBranchAdminForRestoreAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForRestoreAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}