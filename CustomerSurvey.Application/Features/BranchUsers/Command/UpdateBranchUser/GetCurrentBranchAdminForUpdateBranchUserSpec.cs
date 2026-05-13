using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.UpdateBranchUser
{
    internal sealed record CurrentBranchAdminForUpdateBranchUserDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForUpdateBranchUserSpec
        : Specification<BranchAdmin, CurrentBranchAdminForUpdateBranchUserDto>
    {
        public GetCurrentBranchAdminForUpdateBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForUpdateBranchUserDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}