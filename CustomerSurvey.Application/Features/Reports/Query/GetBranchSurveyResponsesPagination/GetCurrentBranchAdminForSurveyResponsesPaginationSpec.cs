using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;

internal sealed class GetCurrentBranchAdminForSurveyResponsesPaginationSpec
    : Specification<BranchAdmin, CurrentBranchActorForSurveyResponsesPaginationDto>
{
    public GetCurrentBranchAdminForSurveyResponsesPaginationSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new CurrentBranchActorForSurveyResponsesPaginationDto
        {
            BranchId = x.BranchId
        });
    }
}