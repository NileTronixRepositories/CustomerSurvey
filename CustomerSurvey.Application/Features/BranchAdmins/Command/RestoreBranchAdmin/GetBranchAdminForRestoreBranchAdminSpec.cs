using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.RestoreBranchAdmin
{
    internal sealed record BranchAdminForRestoreBranchAdminDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }
    }

    internal sealed class GetBranchAdminForRestoreBranchAdminSpec
        : Specification<BranchAdmin, BranchAdminForRestoreBranchAdminDto>
    {
        public GetBranchAdminForRestoreBranchAdminSpec(Guid branchAdminId)
        {
            AddCriteria(x => x.Id == branchAdminId);

            Select(x => new BranchAdminForRestoreBranchAdminDto
            {
                BranchAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId
            });
        }
    }
}
