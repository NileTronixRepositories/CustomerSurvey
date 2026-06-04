using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Shared.BranchScope
{
    internal sealed record AssignedBranchAreaBranchScopeDto
    {
        public Guid BranchAreaBranchId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetAssignedBranchAreaBranchScopeSpec
        : Specification<BranchAreaBranch, AssignedBranchAreaBranchScopeDto>
    {
        public GetAssignedBranchAreaBranchScopeSpec(Guid branchAreaId, Guid branchId)
        {
            AddCriteria(x =>
                x.BranchAreaId == branchAreaId &&
                x.BranchId == branchId &&
                x.Branch.IsActive);

            Select(x => new AssignedBranchAreaBranchScopeDto
            {
                BranchAreaBranchId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}
