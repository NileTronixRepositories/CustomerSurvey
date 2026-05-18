using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsPagination
{
    internal sealed class GetDepartmentsPaginationSpec
        : Specification<Department, DepartmentPaginationItemDto>
    {
        public GetDepartmentsPaginationSpec(SearchParameters searchParameters)
        {
            if (!string.IsNullOrWhiteSpace(searchParameters.SearchText))
            {
                var searchText = searchParameters.SearchText.Trim();

                AddCriteria(x =>
                    x.NameEn.Contains(searchText) ||
                    (x.NameAr != null && x.NameAr.Contains(searchText)));
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

            Select(x => new DepartmentPaginationItemDto
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                IsActive = x.IsActive,
                CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}