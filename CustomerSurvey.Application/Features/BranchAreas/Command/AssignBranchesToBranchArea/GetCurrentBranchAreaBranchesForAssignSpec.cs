using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.AssignBranchesToBranchArea
{
    internal sealed class GetCurrentBranchAreaBranchesForAssignSpec
        : Specification<BranchAreaBranch>
    {
        public GetCurrentBranchAreaBranchesForAssignSpec(Guid branchAreaId)
        {
            AddCriteria(x => x.BranchAreaId == branchAreaId);
        }
    }
}
