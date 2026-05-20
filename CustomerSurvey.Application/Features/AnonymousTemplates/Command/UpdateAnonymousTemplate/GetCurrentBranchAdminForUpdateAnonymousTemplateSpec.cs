using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate
{
    internal sealed class GetCurrentBranchAdminForUpdateAnonymousTemplateSpec
        : Specification<BranchAdmin, CurrentBranchActorForUpdateAnonymousTemplateDto>
    {
        public GetCurrentBranchAdminForUpdateAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForUpdateAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}