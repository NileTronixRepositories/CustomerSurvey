using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.DeleteBranchUser
{
    internal sealed record TargetBranchUserForDeleteBranchUserDto
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetTargetBranchUserForDeleteBranchUserSpec
        : Specification<BranchUser, TargetBranchUserForDeleteBranchUserDto>
    {
        public GetTargetBranchUserForDeleteBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new TargetBranchUserForDeleteBranchUserDto
            {
                BranchUserId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId
            });
        }
    }
}