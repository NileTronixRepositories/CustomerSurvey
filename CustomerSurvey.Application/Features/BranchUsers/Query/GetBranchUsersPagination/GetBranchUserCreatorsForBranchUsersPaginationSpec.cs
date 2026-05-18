using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetBranchUsersPagination
{
    internal sealed class GetBranchUserCreatorsForBranchUsersPaginationSpec
       : Specification<ApplicationUser, BranchUserPaginationCreatorDto>
    {
        public GetBranchUserCreatorsForBranchUsersPaginationSpec(
            IReadOnlyCollection<Guid> applicationUserIds)
        {
            var ids = applicationUserIds
                .Distinct()
                .ToArray();

            AddCriteria(x => ids.Contains(x.Id));

            Select(x => new BranchUserPaginationCreatorDto
            {
                ApplicationUserId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}