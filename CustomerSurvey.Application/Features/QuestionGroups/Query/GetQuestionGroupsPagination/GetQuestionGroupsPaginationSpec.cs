using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination
{
    internal sealed class GetQuestionGroupsPaginationSpec
           : Specification<QuestionGroup, QuestionGroupPaginationItemResponse>
    {
        public GetQuestionGroupsPaginationSpec(
            Guid branchId,
            GetQuestionGroupsPaginationQuery query)
        {
            AddCriteria(x => x.BranchId == branchId);

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
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                IsActive = x.IsActive,
                QuestionsCount = x.Questions.Count,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}