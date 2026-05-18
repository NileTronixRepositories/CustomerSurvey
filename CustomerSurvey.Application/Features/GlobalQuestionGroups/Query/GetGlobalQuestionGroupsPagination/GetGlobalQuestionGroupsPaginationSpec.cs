using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsPagination
{
    internal sealed class GetGlobalQuestionGroupsPaginationSpec
         : Specification<QuestionGroup, GlobalQuestionGroupPaginationItemResponse>
    {
        public GetGlobalQuestionGroupsPaginationSpec(
            GetGlobalQuestionGroupsPaginationQuery query)
        {
            AddCriteria(x =>
                x.Scope == QuestionScope.Global &&
                x.BranchId == null);

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

            Select(x => new GlobalQuestionGroupPaginationItemResponse
            {
                GroupId = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == QuestionScope.Global,
                IsEditable = x.Scope == QuestionScope.Global,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                IsActive = x.IsActive,
                QuestionsCount = x.Questions.Count,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}