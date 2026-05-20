using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetCurrentBranchUserForGetAnonymousTemplateDetailsSpec
        : Specification<BranchUser, CurrentBranchActorForGetAnonymousTemplateDetailsDto>
    {
        public GetCurrentBranchUserForGetAnonymousTemplateDetailsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplateDetailsDto
            {
                BranchId = x.BranchId
            });
        }
    }
}