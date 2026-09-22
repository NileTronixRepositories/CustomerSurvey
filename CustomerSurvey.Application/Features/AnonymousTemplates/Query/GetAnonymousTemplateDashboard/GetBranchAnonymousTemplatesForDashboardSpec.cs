using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetBranchAnonymousTemplatesForDashboardSpec
    : Specification<AnonymousTemplate, AnonymousTemplateDashboardItemDto>
{
    public GetBranchAnonymousTemplatesForDashboardSpec(Guid branchId)
    {
        AddCriteria(x =>
            x.Scope == AnonymousTemplateScope.Branch &&
            x.BranchId == branchId);

        Select(x => new AnonymousTemplateDashboardItemDto
        {
            AnonymousTemplateId = x.Id,
            BranchId = x.BranchId,
            BranchNameEn = x.Branch == null ? null : x.Branch.NameEn,
            BranchNameAr = x.Branch == null ? null : x.Branch.NameAr,
            Scope = x.Scope,
            NameEn = x.NameEn,
            NameAr = x.NameAr,
            IsActive = x.IsActive,
            LogoPath = x.LogoPath,
            PublicUrl = x.PublicUrl,
            QrCode = x.QrCode
        });
    }
}
