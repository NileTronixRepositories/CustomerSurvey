using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;

internal sealed class GetSystemSurveyResponsesPaginationQueryHandler
    : IQueryHandler<GetSystemSurveyResponsesPaginationQuery, Pagination<SystemSurveyResponsePaginationItemResponse>>
{
    private const int DefaultPeriodDays = 30;
    private const int MaxAllowedMonths = 12;
    private const int CustomInputsPreviewCount = 3;

    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetSystemSurveyResponsesPaginationQueryHandler(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        ICurrentUser currentUser)
    {
        _superAdminReadRepository = superAdminReadRepository
            ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

        _surveyResponseReadRepository = surveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));

        _customInputValueReadRepository = customInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<SystemSurveyResponsePaginationItemResponse>>> Handle(
        GetSystemSurveyResponsesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<SystemSurveyResponsePaginationItemResponse>>.Fail(new Error(
                Code: "Reports.SystemResponsesPagination.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == _currentUser.UserId.Value,
            cancellationToken);

        if (!currentSuperAdminExists)
        {
            return Result<Pagination<SystemSurveyResponsePaginationItemResponse>>.Fail(new Error(
                Code: "Reports.SystemResponsesPagination.CurrentSuperAdminNotFound",
                Message: ErrorMessage.GetSystemSurveyResponsesPagination_CurrentSuperAdmin_NotFound,
                Type: ErrorType.NotFound));
        }

        var periodResult = ResolvePeriod(request);

        if (periodResult.Error is not null)
        {
            return Result<Pagination<SystemSurveyResponsePaginationItemResponse>>.Fail(
                periodResult.Error);
        }

        var period = periodResult.Period!;

        var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var spec = new GetSystemSurveyResponsesPaginationSpec(
            fromUtc,
            toExclusiveUtc,
            request);

        var (items, totalCount) = await _surveyResponseReadRepository.ListWithCountAsync(
            spec,
            cancellationToken);

        var responseItems = items.ToArray();

        await AttachCustomInputPreviewsAsync(
            responseItems,
            cancellationToken);

        var response = new Pagination<SystemSurveyResponsePaginationItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: responseItems);

        return Result<Pagination<SystemSurveyResponsePaginationItemResponse>>.Ok(response);
    }

    private static PeriodResolveResult ResolvePeriod(
        GetSystemSurveyResponsesPaginationQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-DefaultPeriodDays);

        if (from > to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.SystemResponsesPagination.DateRangeInvalid",
                Message: ErrorMessage.GetSystemSurveyResponsesPagination_DateRange_Invalid,
                Type: ErrorType.Validation));
        }

        if (from.AddMonths(MaxAllowedMonths) < to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.SystemResponsesPagination.DateRangeTooLarge",
                Message: ErrorMessage.GetSystemSurveyResponsesPagination_DateRange_TooLarge,
                Type: ErrorType.Validation));
        }

        return PeriodResolveResult.Ok(new ResolvedSystemResponsesPaginationPeriod(
            From: from,
            To: to));
    }

    private async Task AttachCustomInputPreviewsAsync(
        IReadOnlyCollection<SystemSurveyResponsePaginationItemResponse> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var responseIds = items
            .Select(x => x.SurveyResponseId)
            .Distinct()
            .ToArray();

        var customInputValues = await _customInputValueReadRepository.ListAsync(
            new GetSystemSurveyResponseCustomInputPreviewsSpec(responseIds),
            cancellationToken);

        var previewsByResponseId = customInputValues
            .GroupBy(x => x.SurveyResponseId)
            .ToDictionary(
                x => x.Key,
                x => x
                    .Take(CustomInputsPreviewCount)
                    .Select(MapCustomInputPreview)
                    .Where(value => !string.IsNullOrWhiteSpace(value.Value))
                    .ToArray());

        foreach (var item in items)
        {
            if (previewsByResponseId.TryGetValue(
                    item.SurveyResponseId,
                    out var previews))
            {
                item.CustomInputsPreview = previews;
            }
        }
    }

    private static SystemSurveyResponseCustomInputPreviewResponse MapCustomInputPreview(
        SystemSurveyResponseCustomInputPreviewDto value)
    {
        var displayValue = value.TypeSnapshot switch
        {
            TemplateCustomInputType.String => value.StringValue ?? string.Empty,
            TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };

        return new SystemSurveyResponseCustomInputPreviewResponse
        {
            Name = value.NameSnapshot,
            Value = displayValue
        };
    }

    private sealed record ResolvedSystemResponsesPaginationPeriod(
        DateOnly From,
        DateOnly To);

    private sealed record PeriodResolveResult(
        ResolvedSystemResponsesPaginationPeriod? Period,
        Error? Error)
    {
        public static PeriodResolveResult Ok(ResolvedSystemResponsesPaginationPeriod period)
        {
            return new PeriodResolveResult(period, null);
        }

        public static PeriodResolveResult Fail(Error error)
        {
            return new PeriodResolveResult(null, error);
        }
    }
}