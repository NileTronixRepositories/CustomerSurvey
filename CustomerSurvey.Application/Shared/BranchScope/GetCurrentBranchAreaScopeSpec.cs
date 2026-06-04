using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Shared.BranchScope
{
    internal sealed record CurrentBranchAreaScopeDto
    {
        public Guid BranchAreaId { get; init; }

        public bool IsActive { get; init; }
    }

    internal sealed class GetCurrentBranchAreaScopeSpec
        : Specification<BranchArea, CurrentBranchAreaScopeDto>
    {
        public GetCurrentBranchAreaScopeSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAreaScopeDto
            {
                BranchAreaId = x.Id,
                IsActive = x.ApplicationUser.IsActive
            });
        }
    }
}
