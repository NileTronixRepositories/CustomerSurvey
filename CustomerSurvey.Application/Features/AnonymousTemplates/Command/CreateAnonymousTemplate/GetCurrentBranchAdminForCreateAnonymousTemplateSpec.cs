using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate
{
    internal sealed class GetCurrentBranchAdminForCreateAnonymousTemplateSpec
        : Specification<BranchAdmin, CurrentBranchActorForCreateAnonymousTemplateDto>
    {
        public GetCurrentBranchAdminForCreateAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForCreateAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}