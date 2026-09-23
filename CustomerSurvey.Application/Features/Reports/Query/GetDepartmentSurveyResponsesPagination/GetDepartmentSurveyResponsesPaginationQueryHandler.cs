using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination;

internal sealed class GetDepartmentSurveyResponsesPaginationQueryHandler
    : IQueryHandler<GetDepartmentSurveyResponsesPaginationQuery, Pagination<DepartmentSurveyResponsePaginationItemResponse>>
{
    private const int DefaultPeriodDays = 30;
    private const int MaxAllowedMonths = 12;
    private const int CustomInputsPreviewCount = 3;

    private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDepartmentSurveyResponsesPaginationQueryHandler(
        IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        ICurrentUser currentUser)
    {
        _departmentAdminReadRepository = departmentAdminReadRepository;
        _surveyResponseReadRepository = surveyResponseReadRepository;
        _customInputValueReadRepository = customInputValueReadRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<Pagination<DepartmentSurveyResponsePaginationItemResponse>>> Handle(
        GetDepartmentSurveyResponsesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<DepartmentSurveyResponsePaginationItemResponse>>.Fail(new Error(
                "Reports.DepartmentResponsesPagination.Unauthenticated",
                ErrorMessage.Auth_Token_Missing,
                ErrorType.Security));
        }

        var currentDepartmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
            new GetCurrentDepartmentAdminForDepartmentDashboardSpec(_currentUser.UserId.Value),
            cancellationToken);

        if (currentDepartmentAdmin is null)
        {
            return Result<Pagination<DepartmentSurveyResponsePaginationItemResponse>>.Fail(new Error(
                "Reports.DepartmentResponsesPagination.CurrentDepartmentAdminNotFound",
                ErrorMessage.GetDepartmentDashboard_CurrentDepartmentAdmin_NotFound,
                ErrorType.NotFound));
        }

        var periodResult = ResolvePeriod(request);
        if (periodResult.Error is not null)
        {
            return Result<Pagination<DepartmentSurveyResponsePaginationItemResponse>>.Fail(periodResult.Error);
        }

        var period = periodResult.Period!;
        var spec = new GetDepartmentSurveyResponsesPaginationSpec(
            currentDepartmentAdmin.DepartmentId,
            period.From.ToDateTime(TimeOnly.MinValue),
            period.To.AddDays(1).ToDateTime(TimeOnly.MinValue),
            request);

        var (items, totalCount) = await _surveyResponseReadRepository.ListWithCountAsync(spec, cancellationToken);
        var responseItems = items.ToArray();
        await AttachCustomInputPreviewsAsync(responseItems, cancellationToken);

        return Result<Pagination<DepartmentSurveyResponsePaginationItemResponse>>.Ok(
            new Pagination<DepartmentSurveyResponsePaginationItemResponse>(
                request.PageNumber,
                request.PageSize,
                totalCount,
                responseItems));
    }

    private static PeriodResolveResult ResolvePeriod(GetDepartmentSurveyResponsesPaginationQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-DefaultPeriodDays);

        if (from > to)
        {
            return PeriodResolveResult.Fail(new Error(
                "Reports.DepartmentResponsesPagination.DateRangeInvalid",
                ErrorMessage.GetDepartmentDashboard_DateRange_Invalid,
                ErrorType.Validation));
        }

        if (from.AddMonths(MaxAllowedMonths) < to)
        {
            return PeriodResolveResult.Fail(new Error(
                "Reports.DepartmentResponsesPagination.DateRangeTooLarge",
                ErrorMessage.GetDepartmentDashboard_DateRange_TooLarge,
                ErrorType.Validation));
        }

        return PeriodResolveResult.Ok(new ResolvedPeriod(from, to));
    }

    private async Task AttachCustomInputPreviewsAsync(
        IReadOnlyCollection<DepartmentSurveyResponsePaginationItemResponse> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var responseIds = items.Select(x => x.SurveyResponseId).ToArray();
        var values = await _customInputValueReadRepository.ListAsync(
            new GetDepartmentSurveyResponseCustomInputPreviewsSpec(responseIds),
            cancellationToken);

        var previewsByResponse = values
            .GroupBy(x => x.SurveyResponseId)
            .ToDictionary(
                x => x.Key,
                x => x.Take(CustomInputsPreviewCount)
                    .Select(MapPreview)
                    .Where(preview => !string.IsNullOrWhiteSpace(preview.Value))
                    .ToArray());

        foreach (var item in items)
        {
            if (previewsByResponse.TryGetValue(item.SurveyResponseId, out var previews))
            {
                item.CustomInputsPreview = previews;
            }
        }
    }

    private static DepartmentSurveyResponseCustomInputPreviewResponse MapPreview(
        DepartmentSurveyResponseCustomInputPreviewDto value)
        => new()
        {
            Name = value.NameSnapshot,
            Value = value.TypeSnapshot switch
            {
                TemplateCustomInputType.String => value.StringValue ?? string.Empty,
                TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
                _ => string.Empty
            }
        };

    private sealed record ResolvedPeriod(DateOnly From, DateOnly To);

    private sealed record PeriodResolveResult(ResolvedPeriod? Period, Error? Error)
    {
        public static PeriodResolveResult Ok(ResolvedPeriod period) => new(period, null);
        public static PeriodResolveResult Fail(Error error) => new(null, error);
    }
}
