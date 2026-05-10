using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    internal sealed record BranchUserForBranchDetailsDto
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }
    }

    internal sealed class GetBranchUsersForBranchDetailsSpec
        : Specification<BranchUser, BranchUserForBranchDetailsDto>
    {
        public GetBranchUsersForBranchDetailsSpec(Guid branchId)
        {
            AddCriteria(x => x.BranchId == branchId);

            AddOrderBy(x => x.ApplicationUser.NameEn);

            Select(x => new BranchUserForBranchDetailsDto
            {
                BranchUserId = x.Id,
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