using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    internal sealed class GetCurrentBranchUserForAnonymousTemplateResponsesPaginationSpec
        : Specification<BranchUser, CurrentBranchActorForGetAnonymousTemplateResponsesPaginationDto>
    {
        public GetCurrentBranchUserForAnonymousTemplateResponsesPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplateResponsesPaginationDto
            {
                BranchId = x.BranchId
            });
        }
    }
}