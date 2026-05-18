using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination
{
    internal sealed class GetQuestionGroupsPaginationSpec
        : Specification<QuestionGroup, QuestionGroupPaginationItemResponse>
    {
        public GetQuestionGroupsPaginationSpec(
            Guid branchId,
            GetQuestionGroupsPaginationQuery query)
        {
            AddCriteria(x =>
                x.Scope == QuestionScope.Branch &&
                x.BranchId == branchId);

            if (query.IsActive.HasValue)
            {
                AddCriteria(x => x.IsActive == query.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                var searchText = query.SearchText.Trim();

                AddCriteria(x =>
                    x.NameEn.Contains(searchText) ||
                    (x.NameAr != null && x.NameAr.Contains(searchText)));
            }

            if (query.OrderSort == OrderSort.Oldest)
            {
                AddOrderBy(x => x.CreatedOnUtc);
            }
            else
            {
                AddOrderByDescending(x => x.CreatedOnUtc);
            }

            EnableTotalCount();

            ApplyPaging(
                query.PageNumber,
                query.PageSize);

            Select(x => new QuestionGroupPaginationItemResponse
            {
                GroupId = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == QuestionScope.Global,

                // This endpoint is branch-management only.
                // Anything returned from here is editable according to endpoint permission.
                IsEditable = x.Scope == QuestionScope.Branch,

                NameEn = x.NameEn,
                NameAr = x.NameAr,
                IsActive = x.IsActive,
                QuestionsCount = x.Questions.Count,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}