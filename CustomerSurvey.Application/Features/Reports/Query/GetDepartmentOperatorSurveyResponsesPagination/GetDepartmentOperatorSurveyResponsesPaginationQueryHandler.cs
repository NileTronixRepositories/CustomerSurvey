using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

internal sealed class GetDepartmentOperatorSurveyResponsesPaginationQueryHandler
    : IQueryHandler<GetDepartmentOperatorSurveyResponsesPaginationQuery, Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>>
{
    private const int DefaultPeriodDays = 30;
    private const int MaxAllowedMonths = 12;
    private const int CustomInputsPreviewCount = 3;

    private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
    private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDepartmentOperatorSurveyResponsesPaginationQueryHandler(
        IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
        IWriteReadRepository<DomainOperator> operatorReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        ICurrentUser currentUser)
    {
        _departmentAdminReadRepository = departmentAdminReadRepository
            ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

        _operatorReadRepository = operatorReadRepository
            ?? throw new ArgumentNullException(nameof(operatorReadRepository));

        _surveyResponseReadRepository = surveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));

        _customInputValueReadRepository = customInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>>> Handle(
        GetDepartmentOperatorSurveyResponsesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>>.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponsesPagination.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentDepartmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
            new GetCurrentDepartmentAdminForDepartmentOperatorResponsesSpec(_currentUser.UserId.Value),
            cancellationToken);

        if (currentDepartmentAdmin is null)
        {
            return Result<Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>>.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponsesPagination.CurrentDepartmentAdminNotFound",
                Message: ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_CurrentDepartmentAdmin_NotFound,
                Type: ErrorType.NotFound));
        }

        var operatorProfile = await _operatorReadRepository.FirstOrDefaultAsync(
            new GetDepartmentOperatorForResponsesSpec(
                request.OperatorId,
                currentDepartmentAdmin.DepartmentId),
            cancellationToken);

        if (operatorProfile is null)
        {
            return Result<Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>>.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponsesPagination.OperatorNotFound",
                Message: ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_Operator_NotFound,
                Type: ErrorType.NotFound));
        }

        var periodResult = ResolvePeriod(request);

        if (periodResult.Error is not null)
        {
            return Result<Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>>.Fail(
                periodResult.Error);
        }

        var period = periodResult.Period!;

        var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var spec = new GetDepartmentOperatorSurveyResponsesPaginationSpec(
            currentDepartmentAdmin.DepartmentId,
            operatorProfile.OperatorId,
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

        var response = new Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: responseItems);

        return Result<Pagination<DepartmentOperatorSurveyResponsePaginationItemResponse>>.Ok(response);
    }

    private static PeriodResolveResult ResolvePeriod(
        GetDepartmentOperatorSurveyResponsesPaginationQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-DefaultPeriodDays);

        if (from > to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponsesPagination.DateRangeInvalid",
                Message: ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_DateRange_Invalid,
                Type: ErrorType.Validation));
        }

        if (from.AddMonths(MaxAllowedMonths) < to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponsesPagination.DateRangeTooLarge",
                Message: ErrorMessage.GetDepartmentOperatorSurveyResponsesPagination_DateRange_TooLarge,
                Type: ErrorType.Validation));
        }

        return PeriodResolveResult.Ok(new ResolvedPaginationPeriod(
            From: from,
            To: to));
    }

    private async Task AttachCustomInputPreviewsAsync(
        IReadOnlyCollection<DepartmentOperatorSurveyResponsePaginationItemResponse> items,
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
            new GetDepartmentOperatorSurveyResponseCustomInputPreviewsSpec(responseIds),
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

    private static DepartmentOperatorSurveyResponseCustomInputPreviewResponse MapCustomInputPreview(
        DepartmentOperatorSurveyResponseCustomInputPreviewDto value)
    {
        var displayValue = value.TypeSnapshot switch
        {
            TemplateCustomInputType.String => value.StringValue ?? string.Empty,
            TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };

        return new DepartmentOperatorSurveyResponseCustomInputPreviewResponse
        {
            Name = value.NameSnapshot,
            Value = displayValue
        };
    }

    private sealed record ResolvedPaginationPeriod(
        DateOnly From,
        DateOnly To);

    private sealed record PeriodResolveResult(
        ResolvedPaginationPeriod? Period,
        Error? Error)
    {
        public static PeriodResolveResult Ok(ResolvedPaginationPeriod period)
        {
            return new PeriodResolveResult(period, null);
        }

        public static PeriodResolveResult Fail(Error error)
        {
            return new PeriodResolveResult(null, error);
        }
    }
}
