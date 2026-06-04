using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Shared.BranchScope
{
    internal sealed record CurrentBranchUserScopeDto
    {
        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchUserScopeSpec
        : Specification<BranchUser, CurrentBranchUserScopeDto>
    {
        public GetCurrentBranchUserScopeSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchUserScopeDto
            {
                BranchUserId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}
