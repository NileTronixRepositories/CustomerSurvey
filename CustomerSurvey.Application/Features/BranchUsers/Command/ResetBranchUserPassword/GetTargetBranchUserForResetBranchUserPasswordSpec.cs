using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.ResetBranchUserPassword
{
    internal sealed record TargetBranchUserForResetBranchUserPasswordDto
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetTargetBranchUserForResetBranchUserPasswordSpec
        : Specification<BranchUser, TargetBranchUserForResetBranchUserPasswordDto>
    {
        public GetTargetBranchUserForResetBranchUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new TargetBranchUserForResetBranchUserPasswordDto
            {
                BranchUserId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId
            });
        }
    }
}