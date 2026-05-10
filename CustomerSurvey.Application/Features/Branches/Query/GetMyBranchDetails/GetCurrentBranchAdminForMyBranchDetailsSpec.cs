using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetMyBranchDetails
{
    internal sealed record CurrentBranchAdminForMyBranchDetailsDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForMyBranchDetailsSpec
        : Specification<BranchAdmin, CurrentBranchAdminForMyBranchDetailsDto>
    {
        public GetCurrentBranchAdminForMyBranchDetailsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForMyBranchDetailsDto
            {
                BranchAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId
            });
        }
    }
}