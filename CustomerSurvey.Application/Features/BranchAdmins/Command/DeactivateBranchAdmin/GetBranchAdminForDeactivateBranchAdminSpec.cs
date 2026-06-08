using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.DeactivateBranchAdmin
{
    internal sealed record BranchAdminForDeactivateBranchAdminDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }
    }

    internal sealed class GetBranchAdminForDeactivateBranchAdminSpec
        : Specification<BranchAdmin, BranchAdminForDeactivateBranchAdminDto>
    {
        public GetBranchAdminForDeactivateBranchAdminSpec(Guid branchAdminId)
        {
            AddCriteria(x => x.Id == branchAdminId);

            Select(x => new BranchAdminForDeactivateBranchAdminDto
            {
                BranchAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId
            });
        }
    }
}
