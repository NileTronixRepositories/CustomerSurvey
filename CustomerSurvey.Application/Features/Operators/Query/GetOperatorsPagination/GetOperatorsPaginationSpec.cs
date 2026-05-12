using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorsPagination
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed class GetOperatorsPaginationSpec
        : Specification<DomainOperator, OperatorPaginationItemResponse>
    {
        public GetOperatorsPaginationSpec(
            SearchParameters searchParameters,
            Guid? departmentId)
        {
            if (departmentId.HasValue)
            {
                AddCriteria(x => x.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchParameters.SearchText))
            {
                var searchText = searchParameters.SearchText.Trim();

                AddCriteria(x =>
                    x.ApplicationUser.NameEn.Contains(searchText) ||
                    (x.ApplicationUser.NameAr != null && x.ApplicationUser.NameAr.Contains(searchText)) ||
                    x.ApplicationUser.UserName.Contains(searchText) ||
                    (x.ApplicationUser.Email != null && x.ApplicationUser.Email.Contains(searchText)) ||
                    (x.ApplicationUser.PhoneNumber != null && x.ApplicationUser.PhoneNumber.Contains(searchText)) ||
                    x.Department.NameEn.Contains(searchText) ||
                    (x.Department.NameAr != null && x.Department.NameAr.Contains(searchText)));
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

            Select(x => new OperatorPaginationItemResponse
            {
                OperatorId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId,
                DepartmentNameEn = x.Department.NameEn,
                DepartmentNameAr = x.Department.NameAr,
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