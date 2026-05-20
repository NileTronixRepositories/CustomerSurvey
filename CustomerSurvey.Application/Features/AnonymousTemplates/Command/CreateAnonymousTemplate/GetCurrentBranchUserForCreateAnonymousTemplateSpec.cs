using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate
{
    internal sealed class GetCurrentBranchUserForCreateAnonymousTemplateSpec
        : Specification<BranchUser, CurrentBranchActorForCreateAnonymousTemplateDto>
    {
        public GetCurrentBranchUserForCreateAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForCreateAnonymousTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}