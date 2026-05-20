using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetCurrentBranchUserForAnonymousTemplateResponseDetailsSpec
        : Specification<BranchUser, CurrentBranchActorForGetAnonymousTemplateResponseDetailsDto>
    {
        public GetCurrentBranchUserForAnonymousTemplateResponseDetailsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplateResponseDetailsDto
            {
                BranchId = x.BranchId
            });
        }
    }
}