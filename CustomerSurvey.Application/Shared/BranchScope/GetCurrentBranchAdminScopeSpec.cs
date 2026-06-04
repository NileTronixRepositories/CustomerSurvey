using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Shared.BranchScope
{
    internal sealed record CurrentBranchAdminScopeDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminScopeSpec
        : Specification<BranchAdmin, CurrentBranchAdminScopeDto>
    {
        public GetCurrentBranchAdminScopeSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminScopeDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}
