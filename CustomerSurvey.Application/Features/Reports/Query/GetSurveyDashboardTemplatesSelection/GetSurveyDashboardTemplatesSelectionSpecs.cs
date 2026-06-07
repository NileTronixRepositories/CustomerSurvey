using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardTemplatesSelection;

internal sealed class GetSurveyDashboardTemplatesSelectionBranchSpec
    : Specification<Branch, SurveyDashboardTemplatesSelectionBranchRow>
{
    public GetSurveyDashboardTemplatesSelectionBranchSpec(Guid branchId)
    {
        UseNoTracking();

        AddCriteria(x => x.Id == branchId);

        Select(x => new SurveyDashboardTemplatesSelectionBranchRow
        {
            BranchId = x.Id
        });
    }
}

internal sealed class GetAuthorizedTemplatesForSurveyDashboardSelectionSpec
    : Specification<Template, SurveyDashboardTemplateSelectionResponse>
{
    public GetAuthorizedTemplatesForSurveyDashboardSelectionSpec(
        Guid branchId,
        string? searchText,
        bool useArabicDisplayName)
    {
        UseNoTracking();

        AddCriteria(x => x.BranchId == branchId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var value = searchText.Trim();

            AddCriteria(x =>
                x.NameEn.Contains(value) ||
                (x.NameAr != null && x.NameAr.Contains(value)));
        }

        AddOrderBy(x => x.NameEn);

        Select(x => new SurveyDashboardTemplateSelectionResponse
        {
            TemplateId = x.Id,
            TemplateKind = SurveyDashboardTemplateKind.Authorized,
            DashboardSource = SurveyDashboardSource.Internal,
            NameEn = x.NameEn,
            NameAr = x.NameAr,
            DisplayName = useArabicDisplayName &&
                          x.NameAr != null &&
                          x.NameAr != string.Empty
                ? x.NameAr
                : x.NameEn,
            BranchId = x.BranchId,
            BranchNameEn = x.Branch.NameEn,
            BranchNameAr = x.Branch.NameAr,
            BranchCode = x.Branch.Code
        });
    }
}

internal sealed class GetAnonymousTemplatesForSurveyDashboardSelectionSpec
    : Specification<AnonymousTemplate, SurveyDashboardTemplateSelectionResponse>
{
    public GetAnonymousTemplatesForSurveyDashboardSelectionSpec(
        Guid branchId,
        string? searchText,
        bool useArabicDisplayName)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.Scope == AnonymousTemplateScope.Branch &&
            x.BranchId == branchId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var value = searchText.Trim();

            AddCriteria(x =>
                x.NameEn.Contains(value) ||
                (x.NameAr != null && x.NameAr.Contains(value)));
        }

        AddOrderBy(x => x.NameEn);

        Select(x => new SurveyDashboardTemplateSelectionResponse
        {
            TemplateId = x.Id,
            TemplateKind = SurveyDashboardTemplateKind.Anonymous,
            DashboardSource = SurveyDashboardSource.Anonymous,
            NameEn = x.NameEn,
            NameAr = x.NameAr,
            DisplayName = useArabicDisplayName &&
                          x.NameAr != null &&
                          x.NameAr != string.Empty
                ? x.NameAr
                : x.NameEn,
            BranchId = x.BranchId!.Value,
            BranchNameEn = x.Branch!.NameEn,
            BranchNameAr = x.Branch.NameAr,
            BranchCode = x.Branch.Code
        });
    }
}

internal sealed record SurveyDashboardTemplatesSelectionBranchRow
{
    public Guid BranchId { get; init; }
}
