using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed class GetSystemDashboardQueryHandler
    : IQueryHandler<GetSystemDashboardQuery, GetSystemDashboardResponse>
{
    private const int DefaultPeriodDays = 30;
    private const int MaxAllowedMonths = 12;
    private const int CriticalCustomInputsPreviewCount = 5;

    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Department> _departmentReadRepository;
    private readonly IWriteReadRepository<Operator> _operatorReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetSystemDashboardQueryHandler(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Department> departmentReadRepository,
        IWriteReadRepository<Operator> operatorReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        ICurrentUser currentUser)
    {
        _superAdminReadRepository = superAdminReadRepository
            ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _departmentReadRepository = departmentReadRepository
            ?? throw new ArgumentNullException(nameof(departmentReadRepository));

        _operatorReadRepository = operatorReadRepository
            ?? throw new ArgumentNullException(nameof(operatorReadRepository));

        _templateReadRepository = templateReadRepository
            ?? throw new ArgumentNullException(nameof(templateReadRepository));

        _surveyResponseReadRepository = surveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));

        _surveyAnswerReadRepository = surveyAnswerReadRepository
            ?? throw new ArgumentNullException(nameof(surveyAnswerReadRepository));

        _customInputValueReadRepository = customInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<GetSystemDashboardResponse>> Handle(
        GetSystemDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetSystemDashboardResponse>.Fail(new Error(
                Code: "Reports.SystemDashboard.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == _currentUser.UserId.Value,
            cancellationToken);

        if (!currentSuperAdminExists)
        {
            return Result<GetSystemDashboardResponse>.Fail(new Error(
                Code: "Reports.SystemDashboard.CurrentSuperAdminNotFound",
                Message: ErrorMessage.GetSystemDashboard_CurrentSuperAdmin_NotFound,
                Type: ErrorType.NotFound));
        }

        var periodResult = ResolvePeriod(request);

        if (periodResult.Error is not null)
        {
            return Result<GetSystemDashboardResponse>.Fail(periodResult.Error);
        }

        var period = periodResult.Period!;

        var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var branches = await _branchReadRepository.ListAsync(
            new GetSystemDashboardBranchesSpec(request.BranchId),
            cancellationToken);

        if (request.BranchId.HasValue && branches.Count == 0)
        {
            return Result<GetSystemDashboardResponse>.Fail(new Error(
                Code: "Reports.SystemDashboard.BranchNotFound",
                Message: ErrorMessage.GetSystemDashboard_Branch_NotFound,
                Type: ErrorType.NotFound));
        }

        var departments = await _departmentReadRepository.ListAsync(
            new GetSystemDashboardDepartmentsSpec(request.DepartmentId),
            cancellationToken);

        if (request.DepartmentId.HasValue && departments.Count == 0)
        {
            return Result<GetSystemDashboardResponse>.Fail(new Error(
                Code: "Reports.SystemDashboard.DepartmentNotFound",
                Message: ErrorMessage.GetSystemDashboard_Department_NotFound,
                Type: ErrorType.NotFound));
        }

        var operators = await _operatorReadRepository.ListAsync(
            new GetSystemDashboardOperatorsSpec(request.DepartmentId),
            cancellationToken);

        var templates = await _templateReadRepository.ListAsync(
            new GetSystemDashboardTemplatesSpec(request.BranchId),
            cancellationToken);

        var responses = await _surveyResponseReadRepository.ListAsync(
            new GetSystemDashboardSurveyResponsesSpec(
                fromUtc,
                toExclusiveUtc,
                request.BranchId,
                request.DepartmentId),
            cancellationToken);

        var answers = await _surveyAnswerReadRepository.ListAsync(
            new GetSystemDashboardSurveyAnswersSpec(
                fromUtc,
                toExclusiveUtc,
                request.BranchId,
                request.DepartmentId),
            cancellationToken);

        var criticalResponseIds = responses
            .Where(x =>
                x.MaxScore > 0 &&
                x.ScorePercentage <= request.CriticalScoreThreshold)
            .OrderBy(x => x.ScorePercentage)
            .ThenByDescending(x => x.SubmittedOnUtc)
            .Take(request.CriticalResponsesCount)
            .Select(x => x.SurveyResponseId)
            .ToArray();

        IReadOnlyCollection<SystemDashboardCustomInputPreviewDto> criticalCustomInputs;

        if (criticalResponseIds.Length == 0)
        {
            criticalCustomInputs = Array.Empty<SystemDashboardCustomInputPreviewDto>();
        }
        else
        {
            criticalCustomInputs = await _customInputValueReadRepository.ListAsync(
                new GetSystemDashboardCustomInputPreviewsSpec(criticalResponseIds),
                cancellationToken);
        }

        var response = BuildResponse(
            request,
            period,
            branches,
            departments,
            operators,
            templates,
            responses,
            answers,
            criticalCustomInputs);

        return Result<GetSystemDashboardResponse>.Ok(response);
    }

    private static PeriodResolveResult ResolvePeriod(GetSystemDashboardQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-DefaultPeriodDays);

        if (from > to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.SystemDashboard.DateRangeInvalid",
                Message: ErrorMessage.GetSystemDashboard_DateRange_Invalid,
                Type: ErrorType.Validation));
        }

        if (from.AddMonths(MaxAllowedMonths) < to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.SystemDashboard.DateRangeTooLarge",
                Message: ErrorMessage.GetSystemDashboard_DateRange_TooLarge,
                Type: ErrorType.Validation));
        }

        return PeriodResolveResult.Ok(new ResolvedSystemDashboardPeriod(
            From: from,
            To: to,
            IsDefaultPeriod: !request.From.HasValue && !request.To.HasValue));
    }

    private static GetSystemDashboardResponse BuildResponse(
        GetSystemDashboardQuery request,
        ResolvedSystemDashboardPeriod period,
        IReadOnlyCollection<SystemDashboardBranchDto> branches,
        IReadOnlyCollection<SystemDashboardDepartmentDto> departments,
        IReadOnlyCollection<SystemDashboardOperatorDto> operators,
        IReadOnlyCollection<SystemDashboardTemplateDto> templates,
        IReadOnlyCollection<SystemDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<SystemDashboardSurveyAnswerDto> answers,
        IReadOnlyCollection<SystemDashboardCustomInputPreviewDto> criticalCustomInputs)
    {
        var scoredResponses = responses
            .Where(x => x.MaxScore > 0)
            .ToArray();

        var averageScore = scoredResponses.Length == 0
            ? 0m
            : Round(scoredResponses.Average(x => x.ScorePercentage));

        var complaintsCount = answers.Count(x => x.QuestionType == QuestionType.Complain);
        var voiceAnswersCount = answers.Count(x => x.QuestionType == QuestionType.Voice);

        return new GetSystemDashboardResponse
        {
            Period = new SystemDashboardPeriodResponse
            {
                From = period.From,
                To = period.To,
                IsDefaultPeriod = period.IsDefaultPeriod,
                GroupBy = request.GroupBy.ToString()
            },

            Summary = new SystemDashboardSummaryResponse
            {
                TotalBranches = branches.Count,
                ActiveBranches = branches.Count(x => x.IsActive),
                InactiveBranches = branches.Count(x => !x.IsActive),

                TotalDepartments = departments.Count,
                ActiveDepartments = departments.Count(x => x.IsActive),

                TotalOperators = operators.Count,
                TotalTemplates = templates.Count,
                ActiveTemplates = templates.Count(x => x.IsActive),

                TotalResponses = responses.Count,
                ScoredResponses = scoredResponses.Length,
                UnscoredResponses = responses.Count - scoredResponses.Length,
                AverageScorePercentage = averageScore,

                SatisfiedResponses = scoredResponses.Count(x => SatisfactionCategoryRule.Matches(
                    x.MaxScore, x.ScorePercentage, SatisfactionCategory.Satisfied)),
                NeutralResponses = scoredResponses.Count(x => SatisfactionCategoryRule.Matches(
                    x.MaxScore, x.ScorePercentage, SatisfactionCategory.Neutral)),
                UnhappyResponses = scoredResponses.Count(x => SatisfactionCategoryRule.Matches(
                    x.MaxScore, x.ScorePercentage, SatisfactionCategory.Unhappy)),

                ComplaintsCount = complaintsCount,
                VoiceAnswersCount = voiceAnswersCount
            },

            Charts = new DashboardChartsResponse
            {
                SatisfactionDistribution = SatisfactionDistributionBuilder.Build(
                    scoredResponses,
                    x => x.ScorePercentage,
                    category => BuildSystemResponsesNavigation(
                        request,
                        period,
                        ("satisfactionCategory", category)))
            },

            SummaryActions = new DashboardSummaryActionsResponse
            {
                AllResponses = BuildSystemResponsesNavigation(request, period),
                Complaints = BuildSystemResponsesNavigation(request, period, ("hasComplaint", true)),
                VoiceAnswers = BuildSystemResponsesNavigation(request, period, ("hasVoice", true))
            },

            SatisfactionTrend = BuildTrend(
                scoredResponses,
                request,
                period),

            BranchPerformance = BuildBranchPerformance(
                branches,
                templates,
                responses,
                answers,
                request,
                period),

            DepartmentActivity = BuildDepartmentActivity(
                departments,
                operators,
                responses,
                request,
                period),

            TopTemplates = BuildTopTemplates(
                templates,
                responses,
                answers,
                request,
                period),

            CriticalResponses = BuildCriticalResponses(
                responses,
                answers,
                criticalCustomInputs,
                request.CriticalScoreThreshold,
                request.CriticalResponsesCount)
        };
    }

    private static IReadOnlyCollection<SystemDashboardTrendPointResponse> BuildTrend(
        IReadOnlyCollection<SystemDashboardSurveyResponseDto> scoredResponses,
        GetSystemDashboardQuery request,
        ResolvedSystemDashboardPeriod period)
    {
        if (request.GroupBy == SystemDashboardGroupBy.Month)
        {
            return scoredResponses
                .GroupBy(x => new
                {
                    x.SubmittedOnUtc.Year,
                    x.SubmittedOnUtc.Month
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(x =>
                {
                    var periodKey = $"{x.Key.Year:D4}-{x.Key.Month:D2}";
                    var range = DashboardDrillDownPathBuilder.ResolveTrendRange(periodKey, true, period.From, period.To);
                    return new SystemDashboardTrendPointResponse
                    {
                        Period = periodKey,
                        ResponsesCount = x.Count(),
                        AverageScorePercentage = Round(x.Average(r => r.ScorePercentage)),
                        DetailsNavigation = BuildSystemResponsesNavigation(
                            request,
                            period,
                            ("from", range.From),
                            ("to", range.To),
                            ("isScored", true))
                    };
                })
                .ToArray();
        }

        return scoredResponses
            .GroupBy(x => DateOnly.FromDateTime(x.SubmittedOnUtc))
            .OrderBy(x => x.Key)
            .Select(x => new SystemDashboardTrendPointResponse
            {
                Period = x.Key.ToString("yyyy-MM-dd"),
                ResponsesCount = x.Count(),
                AverageScorePercentage = Round(x.Average(r => r.ScorePercentage)),
                DetailsNavigation = BuildSystemResponsesNavigation(
                    request,
                    period,
                    ("from", x.Key),
                    ("to", x.Key),
                    ("isScored", true))
            })
            .ToArray();
    }

    private static IReadOnlyCollection<SystemDashboardBranchPerformanceResponse> BuildBranchPerformance(
        IReadOnlyCollection<SystemDashboardBranchDto> branches,
        IReadOnlyCollection<SystemDashboardTemplateDto> templates,
        IReadOnlyCollection<SystemDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<SystemDashboardSurveyAnswerDto> answers,
        GetSystemDashboardQuery request,
        ResolvedSystemDashboardPeriod period)
    {
        var templatesCountByBranchId = templates
            .Where(x => x.IsActive)
            .GroupBy(x => x.BranchId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        var complaintsCountByBranchId = answers
            .Where(x => x.QuestionType == QuestionType.Complain)
            .GroupBy(x => x.BranchId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        var voiceCountByBranchId = answers
            .Where(x => x.QuestionType == QuestionType.Voice)
            .GroupBy(x => x.BranchId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        var responsesByBranchId = responses
            .GroupBy(x => x.BranchId)
            .ToDictionary(
                x => x.Key,
                x => x.ToArray());

        return branches
            .Select(branch =>
            {
                responsesByBranchId.TryGetValue(
                    branch.BranchId,
                    out var branchResponses);

                branchResponses ??= Array.Empty<SystemDashboardSurveyResponseDto>();

                var scored = branchResponses
                    .Where(x => x.MaxScore > 0)
                    .ToArray();

                var average = scored.Length == 0
                    ? 0m
                    : Round(scored.Average(x => x.ScorePercentage));

                templatesCountByBranchId.TryGetValue(branch.BranchId, out var activeTemplatesCount);
                complaintsCountByBranchId.TryGetValue(branch.BranchId, out var complaintsCount);
                voiceCountByBranchId.TryGetValue(branch.BranchId, out var voiceAnswersCount);

                return new SystemDashboardBranchPerformanceResponse
                {
                    BranchId = branch.BranchId,
                    BranchNameEn = branch.NameEn,
                    BranchNameAr = branch.NameAr,
                    BranchCode = branch.Code,
                    ResponsesCount = branchResponses.Length,
                    ScoredResponsesCount = scored.Length,
                    AverageScorePercentage = average,
                    ComplaintsCount = complaintsCount,
                    VoiceAnswersCount = voiceAnswersCount,
                    ActiveTemplatesCount = activeTemplatesCount,
                    RiskLevel = ResolveRiskLevel(average),
                    DetailsNavigation = BuildSystemResponsesNavigation(
                        request,
                        period,
                        ("branchId", branch.BranchId))
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.ResponsesCount)
            .ToArray();
    }

    private static IReadOnlyCollection<SystemDashboardDepartmentActivityResponse> BuildDepartmentActivity(
        IReadOnlyCollection<SystemDashboardDepartmentDto> departments,
        IReadOnlyCollection<SystemDashboardOperatorDto> operators,
        IReadOnlyCollection<SystemDashboardSurveyResponseDto> responses,
        GetSystemDashboardQuery request,
        ResolvedSystemDashboardPeriod period)
    {
        var operatorsCountByDepartmentId = operators
            .GroupBy(x => x.DepartmentId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        var responsesByDepartmentId = responses
            .GroupBy(x => x.DepartmentId)
            .ToDictionary(
                x => x.Key,
                x => x.ToArray());

        return departments
            .Select(department =>
            {
                operatorsCountByDepartmentId.TryGetValue(
                    department.DepartmentId,
                    out var operatorsCount);

                responsesByDepartmentId.TryGetValue(
                    department.DepartmentId,
                    out var departmentResponses);

                departmentResponses ??= Array.Empty<SystemDashboardSurveyResponseDto>();

                return new SystemDashboardDepartmentActivityResponse
                {
                    DepartmentId = department.DepartmentId,
                    DepartmentNameEn = department.NameEn,
                    DepartmentNameAr = department.NameAr,
                    OperatorsCount = operatorsCount,
                    ResponsesCount = departmentResponses.Length,
                    LastResponseOnUtc = departmentResponses.Length == 0
                        ? null
                        : departmentResponses.Max(x => x.SubmittedOnUtc),
                    DetailsNavigation = BuildSystemResponsesNavigation(
                        request,
                        period,
                        ("departmentId", department.DepartmentId))
                };
            })
            .OrderByDescending(x => x.ResponsesCount)
            .ThenBy(x => x.DepartmentNameEn)
            .ToArray();
    }

    private static IReadOnlyCollection<SystemDashboardTopTemplateResponse> BuildTopTemplates(
        IReadOnlyCollection<SystemDashboardTemplateDto> templates,
        IReadOnlyCollection<SystemDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<SystemDashboardSurveyAnswerDto> answers,
        GetSystemDashboardQuery request,
        ResolvedSystemDashboardPeriod period)
    {
        var templatesById = templates.ToDictionary(x => x.TemplateId);

        var complaintsCountByTemplateId = answers
            .Where(x => x.QuestionType == QuestionType.Complain)
            .GroupBy(x => x.TemplateId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        return responses
            .GroupBy(x => x.TemplateId)
            .Where(x => templatesById.ContainsKey(x.Key))
            .Select(x =>
            {
                var template = templatesById[x.Key];

                var scored = x
                    .Where(r => r.MaxScore > 0)
                    .ToArray();

                var average = scored.Length == 0
                    ? 0m
                    : Round(scored.Average(r => r.ScorePercentage));

                complaintsCountByTemplateId.TryGetValue(
                    x.Key,
                    out var complaintsCount);

                return new SystemDashboardTopTemplateResponse
                {
                    TemplateId = template.TemplateId,
                    TemplateNameEn = template.TemplateNameEn,
                    TemplateNameAr = template.TemplateNameAr,
                    BranchId = template.BranchId,
                    BranchNameEn = template.BranchNameEn,
                    BranchNameAr = template.BranchNameAr,
                    ResponsesCount = x.Count(),
                    ScoredResponsesCount = scored.Length,
                    AverageScorePercentage = average,
                    ComplaintsCount = complaintsCount,
                    RiskLevel = ResolveRiskLevel(average),
                    DetailsNavigation = BuildSystemResponsesNavigation(
                        request,
                        period,
                        ("templateId", template.TemplateId))
                };
            })
            .OrderByDescending(x => x.ResponsesCount)
            .ThenBy(x => x.AverageScorePercentage)
            .Take(request.TopTemplatesCount)
            .ToArray();
    }

    private static IReadOnlyCollection<SystemDashboardCriticalResponseItem> BuildCriticalResponses(
        IReadOnlyCollection<SystemDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<SystemDashboardSurveyAnswerDto> answers,
        IReadOnlyCollection<SystemDashboardCustomInputPreviewDto> criticalCustomInputs,
        decimal criticalScoreThreshold,
        int criticalResponsesCount)
    {
        var complaintsByResponseId = answers
            .Where(x =>
                x.QuestionType == QuestionType.Complain &&
                !string.IsNullOrWhiteSpace(x.TextAnswer))
            .GroupBy(x => x.SurveyResponseId)
            .ToDictionary(
                x => x.Key,
                x => x.First().TextAnswer);

        var customInputsByResponseId = criticalCustomInputs
            .GroupBy(x => x.SurveyResponseId)
            .ToDictionary(
                x => x.Key,
                x => x
                    .Take(CriticalCustomInputsPreviewCount)
                    .Select(MapCriticalCustomInput)
                    .Where(value => !string.IsNullOrWhiteSpace(value.Value))
                    .ToArray());

        return responses
            .Where(x =>
                x.MaxScore > 0 &&
                x.ScorePercentage <= criticalScoreThreshold)
            .OrderBy(x => x.ScorePercentage)
            .ThenByDescending(x => x.SubmittedOnUtc)
            .Take(criticalResponsesCount)
            .Select(x =>
            {
                complaintsByResponseId.TryGetValue(
                    x.SurveyResponseId,
                    out var complaintText);

                customInputsByResponseId.TryGetValue(
                    x.SurveyResponseId,
                    out var customInputs);

                return new SystemDashboardCriticalResponseItem
                {
                    SurveyResponseId = x.SurveyResponseId,
                    BranchId = x.BranchId,
                    BranchNameEn = x.BranchNameEn,
                    BranchNameAr = x.BranchNameAr,
                    TemplateId = x.TemplateId,
                    TemplateNameEn = x.TemplateNameEn,
                    TemplateNameAr = x.TemplateNameAr,
                    OperatorId = x.OperatorId,
                    OperatorNameEn = x.OperatorNameEn,
                    OperatorNameAr = x.OperatorNameAr,
                    SubmittedOnUtc = x.SubmittedOnUtc,
                    ScorePercentage = x.ScorePercentage,
                    ComplaintText = complaintText,
                    CustomInputs = customInputs ?? Array.Empty<SystemDashboardCriticalResponseCustomInputItem>(),
                    DetailsNavigation = DashboardDrillDownPathBuilder.Navigation(
                        "SystemResponseDetails",
                        $"/api/reports/system-responses/{x.SurveyResponseId}")
                };
            })
            .ToArray();
    }

    private static SystemDashboardCriticalResponseCustomInputItem MapCriticalCustomInput(
        SystemDashboardCustomInputPreviewDto value)
    {
        var displayValue = value.TypeSnapshot switch
        {
            TemplateCustomInputType.String => value.StringValue ?? string.Empty,
            TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };

        return new SystemDashboardCriticalResponseCustomInputItem
        {
            Name = value.NameSnapshot,
            Value = displayValue
        };
    }

    private static DashboardDetailsNavigationResponse BuildSystemResponsesNavigation(
        GetSystemDashboardQuery request,
        ResolvedSystemDashboardPeriod period,
        params (string Name, object? Value)[] additionalFilters)
    {
        var filters = new List<(string Name, object? Value)>
        {
            ("from", period.From),
            ("to", period.To),
            ("branchId", request.BranchId),
            ("departmentId", request.DepartmentId)
        };

        foreach (var filter in additionalFilters)
        {
            filters.RemoveAll(existing => string.Equals(existing.Name, filter.Name, StringComparison.OrdinalIgnoreCase));
            filters.Add(filter);
        }

        filters.Add(("pageNumber", 1));
        filters.Add(("pageSize", 10));

        return DashboardDrillDownPathBuilder.Navigation(
            "SystemResponses",
            "/api/reports/system-responses",
            filters.ToArray());
    }

    private static string ResolveRiskLevel(decimal averageScorePercentage)
    {
        if (averageScorePercentage < 50m)
        {
            return "HighRisk";
        }

        if (averageScorePercentage < 75m)
        {
            return "MediumRisk";
        }

        return "Healthy";
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private sealed record ResolvedSystemDashboardPeriod(
        DateOnly From,
        DateOnly To,
        bool IsDefaultPeriod);

    private sealed record PeriodResolveResult(
        ResolvedSystemDashboardPeriod? Period,
        Error? Error)
    {
        public static PeriodResolveResult Ok(ResolvedSystemDashboardPeriod period)
        {
            return new PeriodResolveResult(period, null);
        }

        public static PeriodResolveResult Fail(Error error)
        {
            return new PeriodResolveResult(null, error);
        }
    }
}
