using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.UpdateBranchUser
{
    internal sealed record TargetBranchUserForUpdateBranchUserDto
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetTargetBranchUserForUpdateBranchUserSpec
        : Specification<BranchUser, TargetBranchUserForUpdateBranchUserDto>
    {
        public GetTargetBranchUserForUpdateBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new TargetBranchUserForUpdateBranchUserDto
            {
                BranchUserId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId
            });
        }
    }
}