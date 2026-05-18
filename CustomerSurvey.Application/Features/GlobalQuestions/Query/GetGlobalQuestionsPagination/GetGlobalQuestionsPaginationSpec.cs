using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionsPagination
{
    internal sealed class GetGlobalQuestionsPaginationSpec
        : Specification<Question, GlobalQuestionPaginationItemResponse>
    {
        public GetGlobalQuestionsPaginationSpec(
            GetGlobalQuestionsPaginationQuery query)
        {
            AddCriteria(x =>
                x.Scope == QuestionScope.Global &&
                x.BranchId == null &&
                x.Group.Scope == QuestionScope.Global &&
                x.Group.BranchId == null);

            if (query.GroupId.HasValue)
            {
                AddCriteria(x => x.GroupId == query.GroupId.Value);
            }

            if (query.IsActive.HasValue)
            {
                AddCriteria(x => x.IsActive == query.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                var searchText = query.SearchText.Trim();

                AddCriteria(x =>
                    x.TextEn.Contains(searchText) ||
                    (x.TextAr != null && x.TextAr.Contains(searchText)) ||
                    x.Group.NameEn.Contains(searchText) ||
                    (x.Group.NameAr != null && x.Group.NameAr.Contains(searchText)));
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

            Select(x => new GlobalQuestionPaginationItemResponse
            {
                QuestionId = x.Id,
                BranchId = x.BranchId,
                GroupId = x.GroupId,
                GroupBranchId = x.Group.BranchId,
                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == QuestionScope.Global,
                IsEditable = x.Scope == QuestionScope.Global,

                GroupNameEn = x.Group.NameEn,
                GroupNameAr = x.Group.NameAr,

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