using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardComplaints;

internal sealed class GetSurveyDashboardComplaintsQueryHandler
    : IQueryHandler<GetSurveyDashboardComplaintsQuery, SurveyDashboardComplaintsResponse>
{
    private readonly ISurveyDashboardRequestResolver _requestResolver;
    private readonly ISurveyDashboardComplaintReadService _readService;

    public GetSurveyDashboardComplaintsQueryHandler(
        ISurveyDashboardRequestResolver requestResolver,
        ISurveyDashboardComplaintReadService readService)
    {
        _requestResolver = requestResolver;
        _readService = readService;
    }

    public async Task<Result<SurveyDashboardComplaintsResponse>> Handle(
        GetSurveyDashboardComplaintsQuery request,
        CancellationToken cancellationToken)
    {
        var resolution = await _requestResolver.ResolveAsync(request, cancellationToken);
        if (resolution.IsFailure)
        {
            return Result<SurveyDashboardComplaintsResponse>.Fail(resolution.Errors);
        }

        var context = resolution.Value;
        var readResult = await _readService.ReadAsync(
            new SurveyDashboardComplaintReadRequest(
                context.IncludeInternal,
                context.IncludeAnonymous,
                context.Scope.BranchId,
                context.InternalTemplateId,
                context.AnonymousTemplateId,
                context.Period.From.ToDateTime(TimeOnly.MinValue),
                context.Period.To.AddDays(1).ToDateTime(TimeOnly.MinValue),
                request.PageNumber,
                request.PageSize),
            cancellationToken);

        var pageItems = readResult.PageItems
            .Select(x => MapItem(x, context.Actor.IsSuperAdmin))
            .ToArray();
        var pageItemsByTemplate = pageItems
            .GroupBy(x => (x.TemplateKind, x.TemplateId))
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<SurveyDashboardComplaintItemResponse>)x.ToArray());

        var groups = readResult.TemplateGroups.Select(group => new SurveyDashboardComplaintTemplateGroupResponse
        {
            TemplateId = group.TemplateId,
            TemplateKind = group.TemplateKind,
            TemplateNameEn = group.TemplateNameEn,
            TemplateNameAr = group.TemplateNameAr,
            BranchId = group.BranchId,
            BranchNameEn = group.BranchNameEn,
            BranchNameAr = group.BranchNameAr,
            TotalComplaints = group.TotalComplaints,
            ResponsesWithComplaints = group.ResponsesWithComplaints,
            Complaints = pageItemsByTemplate.GetValueOrDefault(
                (group.TemplateKind, group.TemplateId),
                Array.Empty<SurveyDashboardComplaintItemResponse>())
        }).ToArray();

        return Result<SurveyDashboardComplaintsResponse>.Ok(new SurveyDashboardComplaintsResponse
        {
            AppliedFilters = new SurveyDashboardAppliedFiltersResponse
            {
                BranchId = context.Scope.BranchId,
                Source = context.AppliedSource,
                TemplateId = context.TemplateFilter?.TemplateId,
                TemplateKind = context.TemplateFilter?.TemplateKind,
                From = context.Period.From,
                To = context.Period.To
            },
            TotalComplaints = readResult.TotalComplaints,
            ResponsesWithComplaints = readResult.ResponsesWithComplaints,
            TotalResponses = readResult.TotalResponses,
            ComplaintRate = readResult.TotalResponses == 0
                ? 0m
                : ReportScoreRounding.Round(
                    readResult.ResponsesWithComplaints * 100m / readResult.TotalResponses),
            Page = new Pagination<SurveyDashboardComplaintItemResponse>(
                request.PageNumber,
                request.PageSize,
                readResult.TotalComplaints,
                pageItems),
            TemplateGroups = groups
        });
    }

    private static SurveyDashboardComplaintItemResponse MapItem(
        SurveyDashboardComplaintReadItem item,
        bool isSuperAdmin)
        => new()
        {
            ResponseId = item.ResponseId,
            TemplateId = item.TemplateId,
            TemplateKind = item.TemplateKind,
            TemplateNameEn = item.TemplateNameEn,
            TemplateNameAr = item.TemplateNameAr,
            QuestionId = item.QuestionId,
            QuestionTextEn = item.QuestionTextEn,
            QuestionTextAr = item.QuestionTextAr,
            ComplaintText = item.ComplaintText,
            SubmittedOnUtc = item.SubmittedOnUtc,
            BranchId = item.BranchId,
            BranchNameEn = item.BranchNameEn,
            BranchNameAr = item.BranchNameAr,
            Source = item.Source,
            OperatorId = item.OperatorId,
            OperatorNameEn = item.OperatorNameEn,
            OperatorNameAr = item.OperatorNameAr,
            DetailsNavigation = BuildNavigation(item, isSuperAdmin)
        };

    private static SurveyDashboardDetailsNavigationResponse BuildNavigation(
        SurveyDashboardComplaintReadItem item,
        bool isSuperAdmin)
    {
        if (item.Source == SurveyDashboardSource.Anonymous)
        {
            return new SurveyDashboardDetailsNavigationResponse
            {
                RouteType = "AnonymousResponseDetails",
                Path = $"/api/anonymous-templates/{item.TemplateId}/responses/{item.ResponseId}"
            };
        }

        return new SurveyDashboardDetailsNavigationResponse
        {
            RouteType = isSuperAdmin ? "SystemResponseDetails" : "BranchResponseDetails",
            Path = isSuperAdmin
                ? $"/api/reports/system-responses/{item.ResponseId}"
                : $"/api/reports/branch-responses/{item.ResponseId}"
        };
    }
}
