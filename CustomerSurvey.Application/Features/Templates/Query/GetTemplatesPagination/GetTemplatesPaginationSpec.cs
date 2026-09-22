using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    internal sealed class GetTemplatesPaginationSpec
        : Specification<Template, TemplatePaginationItemDto>
    {
        public GetTemplatesPaginationSpec(
            Guid branchId,
            SearchParameters searchParameters,
            bool? isActive = null)
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
                BranchNameEn = x.Branch.NameEn,
                BranchNameAr = x.Branch.NameAr,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,
                IsActive = x.IsActive,
                LogoPath = x.LogoPath,
                QuestionsCount = x.TemplateQuestions.Count,
                CustomInputsCount = x.CustomInputs.Count(customInput => customInput.IsActive),
                CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                CreatedOnUtc = x.CreatedOnUtc,
                ActiveFrom = x.ActiveFrom,
                ExpireTo = x.ExpireTo
            });
        }
    }
}
