using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesPagination
{
    internal sealed class GetBranchesPaginationSpec
        : Specification<Branch, BranchPaginationItemDto>
    {
        public GetBranchesPaginationSpec(SearchParameters searchParameters)
        {
            if (!string.IsNullOrWhiteSpace(searchParameters.SearchText))
            {
                var searchText = searchParameters.SearchText.Trim();

                AddCriteria(x =>
                    x.NameEn.Contains(searchText) ||
                    (x.NameAr != null && x.NameAr.Contains(searchText)) ||
                    x.Code.Contains(searchText) ||
                    (x.Address != null && x.Address.Contains(searchText)));
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

            Select(x => new BranchPaginationItemDto
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Code = x.Code,
                Address = x.Address,
                IsActive = x.IsActive,
                CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}