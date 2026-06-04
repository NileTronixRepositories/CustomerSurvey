using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal sealed record CurrentApplicationUserForTemplatesPdfReportDto
{
    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}

internal sealed class GetCurrentApplicationUserForTemplatesPdfReportSpec
    : Specification<ApplicationUser, CurrentApplicationUserForTemplatesPdfReportDto>
{
    public GetCurrentApplicationUserForTemplatesPdfReportSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.Id == applicationUserId);

        Select(x => new CurrentApplicationUserForTemplatesPdfReportDto
        {
            NameEn = x.NameEn,
            NameAr = x.NameAr
        });
    }
}
