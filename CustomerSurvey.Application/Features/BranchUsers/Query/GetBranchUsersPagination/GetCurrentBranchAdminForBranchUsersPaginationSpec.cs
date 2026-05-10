using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetBranchUsersPagination
{
    internal sealed record CurrentBranchAdminForBranchUsersPaginationDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForBranchUsersPaginationSpec
        : Specification<BranchAdmin, CurrentBranchAdminForBranchUsersPaginationDto>
    {
        public GetCurrentBranchAdminForBranchUsersPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForBranchUsersPaginationDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}