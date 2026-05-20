using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetCurrentBranchAdminForGetAnonymousTemplateDetailsSpec
        : Specification<BranchAdmin, CurrentBranchActorForGetAnonymousTemplateDetailsDto>
    {
        public GetCurrentBranchAdminForGetAnonymousTemplateDetailsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplateDetailsDto
            {
                BranchId = x.BranchId
            });
        }
    }
}