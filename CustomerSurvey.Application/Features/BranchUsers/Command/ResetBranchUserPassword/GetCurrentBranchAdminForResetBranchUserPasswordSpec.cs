using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.ResetBranchUserPassword
{
    internal sealed record CurrentBranchAdminForResetBranchUserPasswordDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForResetBranchUserPasswordSpec
        : Specification<BranchAdmin, CurrentBranchAdminForResetBranchUserPasswordDto>
    {
        public GetCurrentBranchAdminForResetBranchUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForResetBranchUserPasswordDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}