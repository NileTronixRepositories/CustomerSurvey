using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplatesPagination
{
    internal sealed class GetAnonymousTemplatesPaginationSpec
        : Specification<AnonymousTemplate, AnonymousTemplatePaginationItemResponse>
    {
        public GetAnonymousTemplatesPaginationSpec(
            GetAnonymousTemplatesPaginationQuery request,
            bool isSuperAdmin,
            Guid? currentBranchId)
        {
            if (isSuperAdmin)
            {
                if (request.Scope.HasValue)
                {
                    AddCriteria(x => x.Scope == request.Scope.Value);
                }

                if (request.BranchId.HasValue)
                {
                    AddCriteria(x =>
                        x.Scope == AnonymousTemplateScope.Branch &&
                        x.BranchId == request.BranchId.Value);
                }
            }
            else
            {
                AddCriteria(x =>
                    x.Scope == AnonymousTemplateScope.Branch &&
                    x.BranchId == currentBranchId);
            }

            if (request.IsActive.HasValue)
            {
                AddCriteria(x => x.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var searchText = request.SearchText.Trim();

                AddCriteria(x =>
                    x.NameEn.Contains(searchText) ||
                    (x.NameAr != null && x.NameAr.Contains(searchText)) ||
                    (x.Description != null && x.Description.Contains(searchText)));
            }

            if (request.OrderSort == OrderSort.Oldest)
            {
                AddOrderBy(x => x.CreatedOnUtc);
            }
            else
            {
                AddOrderByDescending(x => x.CreatedOnUtc);
            }

            EnableTotalCount();

            ApplyPaging(
                request.PageNumber,
                request.PageSize);

            Select(x => new AnonymousTemplatePaginationItemResponse
            {
                AnonymousTemplateId = x.Id,

                BranchId = x.BranchId,

                BranchNameEn = x.Branch == null
                    ? null
                    : x.Branch.NameEn,

                BranchNameAr = x.Branch == null
                    ? null
                    : x.Branch.NameAr,

                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == AnonymousTemplateScope.Global,

                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,

                ActiveFrom = x.ActiveFrom,
                ExpireTo = x.ExpireTo,

                IsActive = x.IsActive,
                IsArchived = x.IsArchived,
                LogoPath = x.LogoPath,

                PublicUrl = x.PublicUrl,
                QrCode = x.QrCode,

                QuestionsCount = x.Questions.Count,
                CustomInputsCount = x.CustomInputs.Count(i => i.IsActive),

                ResponsesCount = 0,

                CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}
