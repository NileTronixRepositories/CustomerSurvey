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
            Scope = x.Scope,
            NameEn = x.NameEn,
            NameAr = x.NameAr,
            Status = x.Status,
            IsActive = x.IsActive,
            PublicUrl = x.PublicUrl,
            QrCode = x.QrCode
        });
    }
}
