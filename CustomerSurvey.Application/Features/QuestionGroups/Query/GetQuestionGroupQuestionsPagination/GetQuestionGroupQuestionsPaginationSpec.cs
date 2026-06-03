using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    internal sealed class GetQuestionGroupQuestionsPaginationSpec
        : Specification<Question, QuestionByGroupPaginationItemResponse>
    {
        public GetQuestionGroupQuestionsPaginationSpec(
            Guid questionGroupId,
            Guid? questionGroupBranchId,
            GetQuestionGroupQuestionsPaginationQuery query)
        {
            AddCriteria(x =>
                x.GroupId == questionGroupId &&
                x.BranchId == questionGroupBranchId);

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                var searchText = query.SearchText.Trim();

                AddCriteria(x =>
                    x.TextEn.Contains(searchText) ||
                    (x.TextAr != null && x.TextAr.Contains(searchText)));
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

            Select(x => new QuestionByGroupPaginationItemResponse
            {
                Id = x.Id,
                BranchId = x.BranchId,
                GroupId = x.GroupId,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Type = x.Type,
                TypeName = x.Type.ToString(),
                IsActive = x.IsActive,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}
