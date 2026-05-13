using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.DeleteBranchUser
{
    internal sealed record CurrentBranchAdminForDeleteBranchUserDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForDeleteBranchUserSpec
        : Specification<BranchAdmin, CurrentBranchAdminForDeleteBranchUserDto>
    {
        public GetCurrentBranchAdminForDeleteBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForDeleteBranchUserDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}