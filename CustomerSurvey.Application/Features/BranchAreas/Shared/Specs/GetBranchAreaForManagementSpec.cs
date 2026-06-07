using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAreas.Shared.Specs
{
    internal sealed record BranchAreaForManagementDto
    {
        public Guid BranchAreaId { get; init; }

        public Guid ApplicationUserId { get; init; }
    }

    internal sealed class GetBranchAreaForManagementSpec
        : Specification<BranchArea, BranchAreaForManagementDto>
    {
        public GetBranchAreaForManagementSpec(Guid branchAreaId)
        {
            AddCriteria(x => x.Id == branchAreaId);

            Select(x => new BranchAreaForManagementDto
            {
                BranchAreaId = x.Id,
                ApplicationUserId = x.ApplicationUserId
            });
        }
    }
}
