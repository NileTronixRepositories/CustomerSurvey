using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    internal sealed class GetQuestionsPaginationSpec
       : Specification<Question, QuestionPaginationItemResponse>
    {
        public GetQuestionsPaginationSpec(
            Guid branchId,
            GetQuestionsPaginationQuery searchParameters)
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
                    x.TextEn.Contains(searchText) ||
                    (x.TextAr != null && x.TextAr.Contains(searchText)) ||
                    x.Group.NameEn.Contains(searchText) ||
                    (x.Group.NameAr != null && x.Group.NameAr.Contains(searchText)));
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

            Select(x => new QuestionPaginationItemResponse
            {
                QuestionId = x.Id,
                BranchId = x.BranchId,
                GroupId = x.GroupId,
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