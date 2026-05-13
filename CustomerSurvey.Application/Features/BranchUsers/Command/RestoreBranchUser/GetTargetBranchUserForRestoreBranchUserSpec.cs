using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.RestoreBranchUser
{
    internal sealed record TargetBranchUserForRestoreBranchUserDto
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetTargetBranchUserForRestoreBranchUserSpec
        : Specification<BranchUser, TargetBranchUserForRestoreBranchUserDto>
    {
        public GetTargetBranchUserForRestoreBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new TargetBranchUserForRestoreBranchUserDto
            {
                BranchUserId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId
            });
        }
    }
}