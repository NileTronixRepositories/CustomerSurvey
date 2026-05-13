using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.RestoreBranchUser
{
    internal sealed record CurrentBranchAdminForRestoreBranchUserDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForRestoreBranchUserSpec
        : Specification<BranchAdmin, CurrentBranchAdminForRestoreBranchUserDto>
    {
        public GetCurrentBranchAdminForRestoreBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForRestoreBranchUserDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}