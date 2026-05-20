using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;

internal sealed class GetCurrentBranchUserForSurveyResponsesPaginationSpec
    : Specification<BranchUser, CurrentBranchActorForSurveyResponsesPaginationDto>
{
    public GetCurrentBranchUserForSurveyResponsesPaginationSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new CurrentBranchActorForSurveyResponsesPaginationDto
        {
            BranchId = x.BranchId
        });
    }
}