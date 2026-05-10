using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetBranchUsersPagination
{
    internal sealed record BranchUserPaginationFlatDto
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }

    internal sealed class GetBranchUsersPaginationSpec
        : Specification<BranchUser, BranchUserPaginationFlatDto>
    {
        public GetBranchUsersPaginationSpec(
            Guid branchId,
            SearchParameters searchParameters)
        {
            AddCriteria(x => x.BranchId == branchId);

            if (!string.IsNullOrWhiteSpace(searchParameters.SearchText))
            {
                var searchText = searchParameters.SearchText.Trim();

                AddCriteria(x =>
                    x.ApplicationUser.NameEn.Contains(searchText) ||
                    (x.ApplicationUser.NameAr != null && x.ApplicationUser.NameAr.Contains(searchText)) ||
                    x.ApplicationUser.UserName.Contains(searchText) ||
                    (x.ApplicationUser.Email != null && x.ApplicationUser.Email.Contains(searchText)) ||
                    (x.ApplicationUser.PhoneNumber != null && x.ApplicationUser.PhoneNumber.Contains(searchText)));
            }

            if (searchParameters.OrderSort == OrderSort.Oldest)
            {
                AddOrderByDescending(x => x.CreatedOnUtc);
            }
            else
            {
                AddOrderBy(x => x.CreatedOnUtc);
            }

            EnableTotalCount();

            ApplyPaging(
                searchParameters.PageNumber,
                searchParameters.PageSize);

            Select(x => new BranchUserPaginationFlatDto
            {
                BranchUserId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId,
                NameEn = x.ApplicationUser.NameEn,
                NameAr = x.ApplicationUser.NameAr,
                UserName = x.ApplicationUser.UserName,
                Email = x.ApplicationUser.Email ?? string.Empty,
                PhoneNumber = x.ApplicationUser.PhoneNumber,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}