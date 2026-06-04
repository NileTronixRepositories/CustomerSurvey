using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.BranchAreas.Shared;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAreas.Shared.Specs
{
    internal sealed record BranchAreaBranchListItemDto
    {
        public Guid BranchAreaId { get; init; }

        public BranchAreaBranchItemResponse Branch { get; init; } = new();
    }

    internal sealed class GetBranchAreaBranchesByAreaIdsSpec
        : Specification<BranchAreaBranch, BranchAreaBranchListItemDto>
    {
        public GetBranchAreaBranchesByAreaIdsSpec(IReadOnlyCollection<Guid> branchAreaIds)
        {
            AddCriteria(x => branchAreaIds.Contains(x.BranchAreaId));

            Select(x => new BranchAreaBranchListItemDto
            {
                BranchAreaId = x.BranchAreaId,
                Branch = new BranchAreaBranchItemResponse
                {
                    Id = x.BranchId,
                    NameEn = x.Branch.NameEn,
                    NameAr = x.Branch.NameAr,
                    Code = x.Branch.Code
                }
            });
        }
    }
}
