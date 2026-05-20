using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed class GetCurrentBranchUserForSurveyResponseDetailsSpec
    : Specification<BranchUser, CurrentBranchActorForSurveyResponseDetailsDto>
{
    public GetCurrentBranchUserForSurveyResponseDetailsSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new CurrentBranchActorForSurveyResponseDetailsDto
        {
            BranchId = x.BranchId
        });
    }
}