using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    internal sealed class GetBranchAdminsForBranchDetailsSpec
     : Specification<BranchAdmin, BranchDetailsBranchAdminResponse>
    {
        public GetBranchAdminsForBranchDetailsSpec(Guid branchId)
        {
            AddCriteria(x => x.BranchId == branchId);

            AddOrderBy(x => x.ApplicationUser.NameEn);

            Select(x => new BranchDetailsBranchAdminResponse
            {
                BranchAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                NameEn = x.ApplicationUser.NameEn,
                NameAr = x.ApplicationUser.NameAr,
                UserName = x.ApplicationUser.UserName,
                Email = x.ApplicationUser.Email ?? string.Empty,
                PhoneNumber = x.ApplicationUser.PhoneNumber
            });
        }
    }
}