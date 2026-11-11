using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Domain.Enums;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardComplaints;

public sealed class SurveyDashboardFlagFeedbackFilters : GetSurveyDashboardComplaintsQuery
{
    public bool? Answered { get; init; }
    public Guid? FlagId { get; init; }
}

public sealed record GetSurveyDashboardFlagFeedbackQuery(
    SurveyResponseFlagType Type, SurveyDashboardFlagFeedbackFilters Filters)
    : IQuery<SurveyDashboardFlagFeedbackResponse>;

public sealed record SurveyDashboardFlagFeedbackResponse(
    SurveyDashboardAppliedFiltersResponse AppliedFilters,
    int TotalItems,
    int ResponsesWithFlags,
    Pagination<SurveyDashboardComplaintItemResponse> Page);

internal sealed class GetSurveyDashboardFlagFeedbackValidator : AbstractValidator<GetSurveyDashboardFlagFeedbackQuery>
{
    public GetSurveyDashboardFlagFeedbackValidator()
    {
        RuleFor(x => x.Type).Must(type => type is SurveyResponseFlagType.Suggestion or SurveyResponseFlagType.Comment);
        RuleFor(x => x.Filters).SetValidator(new GetSurveyDashboardComplaintsQueryValidator());
    }
}

internal sealed class GetSurveyDashboardFlagFeedbackHandler(
    ISurveyDashboardRequestResolver resolver,
    ISurveyDashboardComplaintReadService reader)
    : IQueryHandler<GetSurveyDashboardFlagFeedbackQuery, SurveyDashboardFlagFeedbackResponse>
{
    public async Task<Result<SurveyDashboardFlagFeedbackResponse>> Handle(GetSurveyDashboardFlagFeedbackQuery request, CancellationToken ct)
    {
        var filters = request.Filters;
        var resolution = await resolver.ResolveAsync(filters, ct);
        if (resolution.IsFailure)
            return Result<SurveyDashboardFlagFeedbackResponse>.Fail(resolution.Errors);

        var context = resolution.Value;
        var result = await reader.ReadAsync(new SurveyDashboardComplaintReadRequest(
            context.IncludeInternal, false, context.Scope.BranchId, context.InternalTemplateId, null,
            context.Period.From.ToDateTime(TimeOnly.MinValue),
            context.Period.To.AddDays(1).ToDateTime(TimeOnly.MinValue),
            filters.PageNumber, filters.PageSize, request.Type, filters.Answered, filters.SearchText, filters.FlagId), ct);

        var applied = new SurveyDashboardAppliedFiltersResponse
        {
            BranchId = context.Scope.BranchId,
            Source = context.AppliedSource,
            TemplateId = context.TemplateFilter?.TemplateId,
            TemplateKind = context.TemplateFilter?.TemplateKind,
            From = context.Period.From,
            To = context.Period.To
        };
        var items = result.PageItems.Select(x => GetSurveyDashboardComplaintsQueryHandler.MapItem(x, context.Actor.IsSuperAdmin))
            .ToArray();
        return Result<SurveyDashboardFlagFeedbackResponse>.Ok(new(applied,
            result.TotalComplaints, result.ResponsesWithComplaints,
            new Pagination<SurveyDashboardComplaintItemResponse>(
                filters.PageNumber, filters.PageSize, result.TotalComplaints, items)));
    }
}
