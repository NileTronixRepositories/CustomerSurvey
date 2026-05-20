using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplate
{
    internal sealed class GetCurrentBranchUserForDeleteAnonymousTemplateSpec
        : Specification<BranchUser, CurrentBranchActorForDeleteAnonymousTemplateDto>
    {
        public GetCurrentBranchUserForDeleteAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForDeleteAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}