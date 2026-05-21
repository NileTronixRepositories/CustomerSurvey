using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport.Specs;

internal sealed class GetCurrentBranchUserForTemplatesPdfReportSpec
    : Specification<BranchUser, CurrentBranchReportActorDto>
{
    public GetCurrentBranchUserForTemplatesPdfReportSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new CurrentBranchReportActorDto
        {
            ApplicationUserId = x.ApplicationUserId,
            BranchId = x.BranchId,
            NameEn = x.ApplicationUser.NameEn,
            NameAr = x.ApplicationUser.NameAr
        });
    }
}