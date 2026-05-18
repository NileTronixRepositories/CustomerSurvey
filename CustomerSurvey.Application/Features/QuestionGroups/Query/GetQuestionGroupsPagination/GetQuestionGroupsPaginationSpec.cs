using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination
{
    internal sealed class GetQuestionGroupsPaginationSpec
        : Specification<QuestionGroup, QuestionGroupPaginationItemDto>
    {
        public GetQuestionGroupsPaginationSpec(
            Guid branchId,
            GetQuestionGroupsPaginationQuery searchParameters)
        {
            AddCriteria(x => x.BranchId == branchId);

            if (searchParameters.IsActive.HasValue)
            {
                AddCriteria(x => x.IsActive == searchParameters.IsActive.Value);
            }

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

            Select(x => new QuestionGroupPaginationItemDto
            {
                GroupId = x.Id,
                BranchId = x.BranchId,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                IsActive = x.IsActive,
                QuestionsCount = x.Questions.Count,
                CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}