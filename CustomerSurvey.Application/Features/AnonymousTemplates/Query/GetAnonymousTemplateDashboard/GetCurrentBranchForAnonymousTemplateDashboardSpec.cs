using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetCurrentBranchForAnonymousTemplateDashboardSpec
    : Specification<Branch, CurrentBranchActorForAnonymousTemplateDashboardDto>
{
    public GetCurrentBranchForAnonymousTemplateDashboardSpec(Guid branchId)
    {
        AddCriteria(x => x.Id == branchId);

        Select(x => new CurrentBranchActorForAnonymousTemplateDashboardDto
        {
            BranchId = x.Id,
            BranchNameEn = x.NameEn,
            BranchNameAr = x.NameAr
        });
    }
}
