using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetCurrentBranchAdminForAnonymousTemplateResponseDetailsSpec
        : Specification<BranchAdmin, CurrentBranchActorForGetAnonymousTemplateResponseDetailsDto>
    {
        public GetCurrentBranchAdminForAnonymousTemplateResponseDetailsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplateResponseDetailsDto
            {
                BranchId = x.BranchId
            });
        }
    }
}