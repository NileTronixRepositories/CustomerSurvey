using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreasPagination
{
    internal sealed record BranchAreaPaginationFlatDto
    {
        public Guid BranchAreaId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }

    internal sealed class GetBranchAreasPaginationSpec
        : Specification<BranchArea, BranchAreaPaginationFlatDto>
    {
        public GetBranchAreasPaginationSpec(SearchParameters searchParameters)
        {
            if (!string.IsNullOrWhiteSpace(searchParameters.SearchText))
            {
                var searchText = searchParameters.SearchText.Trim();

                AddCriteria(x =>
                    x.ApplicationUser.NameEn.Contains(searchText) ||
                    (x.ApplicationUser.NameAr != null && x.ApplicationUser.NameAr.Contains(searchText)) ||
                    x.ApplicationUser.UserName.Contains(searchText) ||
                    x.ApplicationUser.Email.Contains(searchText) ||
                    (x.ApplicationUser.PhoneNumber != null && x.ApplicationUser.PhoneNumber.Contains(searchText)));
            }

            if (searchParameters.OrderSort == OrderSort.Oldest)
            {
                AddOrderBy(x => x.CreatedOnUtc);
            }
            else
            {
                AddOrderByDescending(x => x.CreatedOnUtc);
            }

            EnableTotalCount();

            ApplyPaging(
                searchParameters.PageNumber,
                searchParameters.PageSize);

            Select(x => new BranchAreaPaginationFlatDto
            {
                BranchAreaId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                NameEn = x.ApplicationUser.NameEn,
                NameAr = x.ApplicationUser.NameAr,
                UserName = x.ApplicationUser.UserName,
                Email = x.ApplicationUser.Email,
                PhoneNumber = x.ApplicationUser.PhoneNumber,
                IsActive = x.ApplicationUser.IsActive,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}
