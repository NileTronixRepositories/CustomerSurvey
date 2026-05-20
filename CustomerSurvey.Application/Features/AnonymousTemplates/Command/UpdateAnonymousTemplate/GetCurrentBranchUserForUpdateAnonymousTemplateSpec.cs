using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate
{
    internal sealed class GetCurrentBranchUserForUpdateAnonymousTemplateSpec
        : Specification<BranchUser, CurrentBranchActorForUpdateAnonymousTemplateDto>
    {
        public GetCurrentBranchUserForUpdateAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForUpdateAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}