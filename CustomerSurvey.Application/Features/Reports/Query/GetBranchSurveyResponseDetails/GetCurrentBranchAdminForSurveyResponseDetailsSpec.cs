using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed class GetCurrentBranchAdminForSurveyResponseDetailsSpec
    : Specification<BranchAdmin, CurrentBranchActorForSurveyResponseDetailsDto>
{
    public GetCurrentBranchAdminForSurveyResponseDetailsSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new CurrentBranchActorForSurveyResponseDetailsDto
        {
            BranchId = x.BranchId
        });
    }
}