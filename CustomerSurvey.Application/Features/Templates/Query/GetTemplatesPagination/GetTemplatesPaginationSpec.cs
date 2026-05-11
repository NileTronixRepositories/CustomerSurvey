using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    internal sealed class GetTemplatesPaginationSpec
        : Specification<Template, TemplatePaginationItemDto>
    {
        public GetTemplatesPaginationSpec(
            Guid branchId,
            SearchParameters searchParameters, bool? isActive = null)
        {
            AddCriteria(x => x.BranchId == branchId);

            if (!string.IsNullOrWhiteSpace(searchParameters.SearchText))
            {
                var searchText = searchParameters.SearchText.Trim();

                AddCriteria(x =>
                    x.NameEn.Contains(searchText) ||
                    (x.NameAr != null && x.NameAr.Contains(searchText)) ||
                    (x.Description != null && x.Description.Contains(searchText)));
            }
            if (isActive is not null)
            {
                AddCriteria(x => x.IsActive == isActive.Value);
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

            Select(x => new TemplatePaginationItemDto
            {
                TemplateId = x.Id,
                BranchId = x.BranchId,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,
                Status = x.Status,
                IsActive = x.IsActive,
                QuestionsCount = x.TemplateQuestions.Count,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}