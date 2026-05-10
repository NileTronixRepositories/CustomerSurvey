using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.CreateBranchUser
{
    internal sealed record CurrentBranchAdminForCreateBranchUserDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForCreateBranchUserSpec
        : Specification<BranchAdmin, CurrentBranchAdminForCreateBranchUserDto>
    {
        public GetCurrentBranchAdminForCreateBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForCreateBranchUserDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}