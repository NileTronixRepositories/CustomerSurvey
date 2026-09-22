using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;

internal sealed class GetSuperAdminAnonymousTemplatesPaginationSpec
    : Specification<AnonymousTemplate, SuperAdminTemplatePaginationItemDto>
{
    public GetSuperAdminAnonymousTemplatesPaginationSpec(
        GetSuperAdminTemplatesPaginationQuery request)
    {
        AddCriteria(x => x.Scope == AnonymousTemplateScope.Branch);

        if (request.BranchId.HasValue)
        {
            AddCriteria(x => x.BranchId == request.BranchId.Value);
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

        Select(x => new SuperAdminTemplatePaginationItemDto
        {
            TemplateId = x.Id,
            BranchId = x.BranchId!.Value,
            BranchNameEn = x.Branch == null
                ? null
                : x.Branch.NameEn,
            BranchNameAr = x.Branch == null
                ? null
                : x.Branch.NameAr,
            TemplateKind = TemplateCatalogKind.Anonymous,
            NameEn = x.NameEn,
            NameAr = x.NameAr,
            Description = x.Description,
            IsActive = x.IsActive,
            LogoPath = x.LogoPath,
            QuestionsCount = x.Questions.Count,
            CustomInputsCount = x.CustomInputs.Count(customInput => customInput.IsActive),
            PublicUrl = x.PublicUrl,
            QrCode = x.QrCode,
            CreatedByApplicationUserId = x.CreatedByApplicationUserId,
            CreatedOnUtc = x.CreatedOnUtc,
            ActiveFrom = x.ActiveFrom,
            ExpireTo = x.ExpireTo
        });
    }
}
