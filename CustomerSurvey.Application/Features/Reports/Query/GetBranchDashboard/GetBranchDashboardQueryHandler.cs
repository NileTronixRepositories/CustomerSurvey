using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed class GetBranchDashboardQueryHandler
    : IQueryHandler<GetBranchDashboardQuery, GetBranchDashboardResponse>
{
    private const int DefaultPeriodDays = 30;
    private const int MaxAllowedMonths = 12;
    private const int MaxCustomInputsToReturn = 5;
    private const int MaxSegmentsPerCustomInput = 10;

    private readonly CustomerSurvey.Application.Abstraction.Reports.ITemplateAvailabilityReader? _availabilityReader;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
    private readonly ICurrentUser _currentUser;

    public GetBranchDashboardQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        ICurrentUser currentUser,
        CustomerSurvey.Application.Abstraction.Reports.ITemplateAvailabilityReader? availabilityReader = null)
    {
        _availabilityReader = availabilityReader;
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _templateReadRepository = templateReadRepository
            ?? throw new ArgumentNullException(nameof(templateReadRepository));

        _surveyResponseReadRepository = surveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));

        _surveyAnswerReadRepository = surveyAnswerReadRepository
            ?? throw new ArgumentNullException(nameof(surveyAnswerReadRepository));

        _customInputValueReadRepository = customInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

        _currentBranchScopeResolver = currentBranchScopeResolver
            ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<GetBranchDashboardResponse>> Handle(
        GetBranchDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetBranchDashboardResponse>.Fail(new Error(
                Code: "Reports.BranchDashboard.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
            cancellationToken);

        if (currentBranchScope.IsFailure)
        {
            return Result<GetBranchDashboardResponse>.Fail(currentBranchScope.Errors);
        }

        var currentActor = await _branchReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchForBranchDashboardSpec(currentBranchScope.Value.BranchId),
            cancellationToken);

        if (currentActor is null)
        {
            return Result<GetBranchDashboardResponse>.Fail(new Error(
                Code: "Reports.BranchDashboard.CurrentBranchActorNotFound",
                Message: ErrorMessage.GetBranchDashboard_CurrentBranchActor_NotFound,
                Type: ErrorType.NotFound));
        }

        if (request.TemplateId.HasValue)
        {
            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForBranchDashboardSpec(
                    request.TemplateId.Value,
                    currentActor.BranchId),
                cancellationToken);

            if (template is null)
            {
                return Result<GetBranchDashboardResponse>.Fail(new Error(
                    Code: "Reports.BranchDashboard.TemplateNotFound",
                    Message: ErrorMessage.GetBranchDashboard_Template_NotFound,
                    Type: ErrorType.NotFound));
            }
        }

        var periodResult = ResolvePeriod(request);

        if (periodResult.Error is not null)
        {
            return Result<GetBranchDashboardResponse>.Fail(periodResult.Error);
        }

        var period = periodResult.Period!;

        var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var templates = await _templateReadRepository.ListAsync(
            new GetBranchTemplatesForDashboardSpec(currentActor.BranchId, request.TemplateId),
            cancellationToken);

        var responses = await _surveyResponseReadRepository.ListAsync(
            new GetDashboardSurveyResponsesSpec(
                currentActor.BranchId,
                fromUtc,
                toExclusiveUtc,
                request.TemplateId),
            cancellationToken);

        var answers = await _surveyAnswerReadRepository.ListAsync(
            new GetDashboardSurveyAnswersSpec(
                currentActor.BranchId,
                fromUtc,
                toExclusiveUtc,
                request.TemplateId),
            cancellationToken);

        var customInputValues = await _customInputValueReadRepository.ListAsync(
            new GetDashboardCustomInputValuesSpec(
                currentActor.BranchId,
                fromUtc,
                toExclusiveUtc,
                request.TemplateId),
            cancellationToken);

        var response = BuildResponse(
            request,
            currentActor,
            period,
            templates,
            responses,
            answers,
            customInputValues);

        if (_availabilityReader is not null)
            response = response with { Availability = await _availabilityReader.ReadAsync(currentActor.BranchId, 1, request.TemplateId,
                fromUtc, toExclusiveUtc, cancellationToken) };

        response = response with { Availability = response.Availability with { TemplatesWithResponsesCount = responses.Select(x => x.TemplateId).Distinct().Count() } };

        return Result<GetBranchDashboardResponse>.Ok(response);
    }

    private static PeriodResolveResult ResolvePeriod(GetBranchDashboardQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-DefaultPeriodDays);

        if (from > to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.BranchDashboard.DateRangeInvalid",
                Message: ErrorMessage.GetBranchDashboard_DateRange_Invalid,
                Type: ErrorType.Validation));
        }

        if (from.AddMonths(MaxAllowedMonths) < to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.BranchDashboard.DateRangeTooLarge",
                Message: ErrorMessage.GetBranchDashboard_DateRange_TooLarge,
                Type: ErrorType.Validation));
        }

        return PeriodResolveResult.Ok(new ResolvedDashboardPeriod(
            From: from,
            To: to,
            IsDefaultPeriod: !request.From.HasValue && !request.To.HasValue));
    }

    private static GetBranchDashboardResponse BuildResponse(
        GetBranchDashboardQuery request,
        CurrentBranchActorForBranchDashboardDto currentActor,
        ResolvedDashboardPeriod period,
        IReadOnlyCollection<TemplateDashboardItemDto> templates,
        IReadOnlyCollection<DashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DashboardSurveyAnswerDto> answers,
        IReadOnlyCollection<DashboardCustomInputValueDto> customInputValues)
    {
        responses = responses.Select(x =>
        {
            var selected = DashboardScoreSelection.Select(x.SnapshotPayload, x.MaxScore, x.ScorePercentage, request.ScoreCalculationMode);
            return x with { MaxScore = selected.MaxScore, ScorePercentage = selected.ScorePercentage,
                RequestedModeAvailable = selected.RequestedModeAvailable, ScoreDerivation = selected.ScoreDerivation };
        }).ToArray();
        var scoringResponses = responses.ToDictionary(x => x.SurveyResponseId);
        customInputValues = customInputValues.Select(x =>
        {
            var scored = scoringResponses.GetValueOrDefault(x.SurveyResponseId);
            return x with { MaxScore = scored?.MaxScore ?? 0, ScorePercentage = scored?.ScorePercentage ?? 0m };
        }).ToArray();
        answers = answers.Select(x =>
        {
            var saved = scoringResponses.GetValueOrDefault(x.SurveyResponseId);
            var token = DashboardScoreSelection.Question(saved?.SnapshotPayload, x.QuestionId, request.ScoreCalculationMode);
            var question = token?.HistoricalQuestion;
            return x with { SubmittedOnUtc = saved?.SubmittedOnUtc ?? DateTime.MinValue, ReportScoreValue = token?.ScoreValue,
                QuestionTextEn = question?.TextEn ?? x.QuestionTextEn, QuestionTextAr = question?.TextAr ?? x.QuestionTextAr };
        }).ToArray();

        var scoredResponses = responses
            .Where(x => x.MaxScore > 0)
            .ToArray();

        var totalResponses = responses.Count;
        var scoredResponsesCount = scoredResponses.Length;
        var unscoredResponsesCount = totalResponses - scoredResponsesCount;

        var averageScore = scoredResponsesCount == 0
            ? 0m
            : Round(scoredResponses.Average(x => x.ScorePercentage));

        var satisfiedResponses = scoredResponses.Count(x =>
            SatisfactionCategoryRule.Matches(x.MaxScore, x.ScorePercentage, SatisfactionCategory.Satisfied, x.Criteria));
        var neutralResponses = scoredResponses.Count(x =>
            SatisfactionCategoryRule.Matches(x.MaxScore, x.ScorePercentage, SatisfactionCategory.Neutral, x.Criteria));
        var unhappyResponses = scoredResponses.Count(x =>
            SatisfactionCategoryRule.Matches(x.MaxScore, x.ScorePercentage, SatisfactionCategory.Unhappy, x.Criteria));

        var complaintsCount = answers.Count(x =>
            x.QuestionType == QuestionType.Complain &&
            !string.IsNullOrWhiteSpace(x.TextAnswer));
        var voiceAnswersCount = answers.Count(x => x.QuestionType == QuestionType.Voice);

        return new GetBranchDashboardResponse
        {
            HasMixedClassificationCriteria = responses.Where(x => x.MaxScore > 0).Select(x => x.Criteria).Distinct().Skip(1).Any(),
            ScoreCalculationMode = request.ScoreCalculationMode,
            StoredBaselineResponsesCount = responses.Count(x => x.ScoreDerivation == "StoredRootBaseline"),
            RequestedModeAvailable = responses.All(x => x.RequestedModeAvailable),
            Period = new BranchDashboardPeriodResponse
            {
                From = period.From,
                To = period.To,
                IsDefaultPeriod = period.IsDefaultPeriod,
                GroupBy = request.GroupBy.ToString()
            },

            Summary = new BranchDashboardSummaryResponse
            {
                BranchId = currentActor.BranchId,
                BranchNameEn = currentActor.BranchNameEn,
                BranchNameAr = currentActor.BranchNameAr,

                TotalResponses = totalResponses,
                ScoredResponses = scoredResponsesCount,
                UnscoredResponses = unscoredResponsesCount,
                AverageScorePercentage = averageScore,

                SatisfiedResponses = satisfiedResponses,
                NeutralResponses = neutralResponses,
                UnhappyResponses = unhappyResponses,

                ActiveTemplatesCount = templates.Count(x => x.IsActive),
                TemplatesWithResponsesCount = responses
                    .Select(x => x.TemplateId)
                    .Distinct()
                    .Count(),

                ComplaintsCount = complaintsCount,
                VoiceAnswersCount = voiceAnswersCount
            },

            Charts = new DashboardChartsResponse
            {
                SatisfactionDistribution = SatisfactionDistributionBuilder.Build(
                    scoredResponses,
                    x => x.ScorePercentage,
                    category => BuildResponsesNavigation(
                        request,
                        period,
                        ("satisfactionCategory", category)))
            },

            SummaryActions = new DashboardSummaryActionsResponse
            {
                ScoredResponses = BuildResponsesNavigation(request, period, ("isScored", true)),
                AllResponses = BuildResponsesNavigation(request, period),
                Complaints = DashboardDrillDownPathBuilder.Navigation("SurveyComplaints", "/api/reports/survey-dashboard/complaints",
                    ("source", "Internal"), ("templateId", request.TemplateId), ("from", period.From), ("to", period.To)),
                VoiceAnswers = BuildResponsesNavigation(request, period, ("hasVoice", true))
            },

            SatisfactionTrend = BuildTrend(
                scoredResponses,
                request,
                period),

            TemplatePerformance = BuildTemplatePerformance(
                responses,
                answers,
                request,
                period),

            LowestRatedQuestions = BuildLowestRatedQuestions(
                answers,
                request,
                period),

            BestRatedQuestions = BuildLowestRatedQuestions(
                answers,
                request,
                period, best: true),

            CustomInputSegments = BuildCustomInputSegments(
                customInputValues,
                request,
                period),

            CriticalResponses = BuildCriticalResponses(
                responses,
                answers,
                customInputValues,
                request.CriticalScoreThreshold,
                request.CriticalResponsesCount)
        };
    }

    private static IReadOnlyCollection<BranchDashboardTrendPointResponse> BuildTrend(
        IReadOnlyCollection<DashboardSurveyResponseDto> scoredResponses,
        GetBranchDashboardQuery request,
        ResolvedDashboardPeriod period)
    {
        if (request.GroupBy == BranchDashboardGroupBy.Month)
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
                    var range = DashboardDrillDownPathBuilder.ResolveTrendRange(
                        periodKey, true, period.From, period.To);
                    return new BranchDashboardTrendPointResponse
                    {
                        Period = periodKey,
                        ResponsesCount = x.Count(),
                        AverageScorePercentage = Round(x.Average(r => r.ScorePercentage)),
                        DetailsNavigation = BuildResponsesNavigation(
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
            .Select(x => new BranchDashboardTrendPointResponse
            {
                Period = x.Key.ToString("yyyy-MM-dd"),
                ResponsesCount = x.Count(),
                AverageScorePercentage = Round(x.Average(r => r.ScorePercentage)),
                DetailsNavigation = BuildResponsesNavigation(
                    request,
                    period,
                    ("from", x.Key),
                    ("to", x.Key),
                    ("isScored", true))
            })
            .ToArray();
    }

    private static IReadOnlyCollection<BranchDashboardTemplatePerformanceResponse> BuildTemplatePerformance(
        IReadOnlyCollection<DashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DashboardSurveyAnswerDto> answers,
        GetBranchDashboardQuery request,
        ResolvedDashboardPeriod period)
    {
        var complaintsCountByTemplateId = answers
            .Where(x =>
                x.QuestionType == QuestionType.Complain &&
                !string.IsNullOrWhiteSpace(x.TextAnswer))
            .GroupBy(x => x.TemplateId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        return responses
            .GroupBy(x => new
            {
                x.TemplateId,
                x.TemplateNameEn,
                x.TemplateNameAr
            })
            .Select(x =>
            {
                var scored = x
                    .Where(r => r.MaxScore > 0)
                    .ToArray();

                var average = scored.Length == 0
                    ? 0m
                    : Round(scored.Average(r => r.ScorePercentage));

                complaintsCountByTemplateId.TryGetValue(
                    x.Key.TemplateId,
                    out var complaintsCount);

                return new BranchDashboardTemplatePerformanceResponse
                {
                    TemplateId = x.Key.TemplateId,
                    TemplateNameEn = x.Key.TemplateNameEn,
                    TemplateNameAr = x.Key.TemplateNameAr,
                    ResponsesCount = x.Count(),
                    ScoredResponsesCount = scored.Length,
                    AverageScorePercentage = average,
                    ComplaintsCount = complaintsCount,
                    RiskLevel = CustomerSurvey.Application.Features.Reports.Shared.EvaluationCriteria.Combine(scored.Select(x => x.Criteria)).Risk(scored.Length == 0 ? null : average),
                    DetailsNavigation = BuildResponsesNavigation(
                        request,
                        period,
                        ("templateId", x.Key.TemplateId))
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.ResponsesCount)
            .ToArray();
    }

    private static IReadOnlyCollection<BranchDashboardQuestionInsightResponse> BuildLowestRatedQuestions(
        IReadOnlyCollection<DashboardSurveyAnswerDto> answers,
        GetBranchDashboardQuery request,
        ResolvedDashboardPeriod period, bool best = false)
    {
        var ranked = answers.OrderByDescending(x => x.SubmittedOnUtc).ThenBy(x => x.SurveyResponseId)
            .Where(x =>
                x.ReportScoreValue.HasValue)
            .Select(x => new
            {
                Answer = x,
                Value = x.ReportScoreValue
            })
            .Where(x => x.Value.HasValue)
            .GroupBy(x => new { x.Answer.TemplateId, x.Answer.QuestionId })
            .Select(x =>
            {
                var sample = x.First().Answer;
                var averageValue = Round(x.Average(a => (decimal)a.Value!.Value));
                return new BranchDashboardQuestionInsightResponse
                {
                    TemplateId = x.Key.TemplateId,
                    TemplateNameEn = sample.TemplateNameEn,
                    TemplateNameAr = sample.TemplateNameAr,
                    QuestionId = x.Key.QuestionId,
                    QuestionTextEn = sample.QuestionTextEn,
                    QuestionTextAr = sample.QuestionTextAr,
                    QuestionType = sample.QuestionType,
                    QuestionTypeName = sample.QuestionType.ToString(),
                    AnswersCount = x.Count(),
                    AverageValue = averageValue,
                    AverageScorePercentage = Round(averageValue / 5m * 100m),
                    DetailsNavigation = BuildResponsesNavigation(
                        request,
                        period,
                        ("templateId", x.Key.TemplateId),
                        ("questionId", x.Key.QuestionId),
                        ("scoreIncludedOnly", true))
                };
            })
            .ToArray();
        return (best
                ? ranked.Where(x => x.AverageScorePercentage >= 70m).OrderByDescending(x => x.AverageScorePercentage)
                : ranked.OrderBy(x => x.AverageScorePercentage))
            .ThenByDescending(x => x.AnswersCount)
            .ThenBy(x => x.TemplateId)
            .ThenBy(x => x.QuestionId)
            .Take(request.TopQuestionsCount)
            .ToArray();
    }

    private static IReadOnlyCollection<BranchDashboardCustomInputSegmentResponse> BuildCustomInputSegments(
        IReadOnlyCollection<DashboardCustomInputValueDto> customInputValues,
        GetBranchDashboardQuery request,
        ResolvedDashboardPeriod period)
    {
        return customInputValues
            .Where(x => x.MaxScore > 0)
            .GroupBy(x => new
            {
                x.CustomInputId,
                x.LabelEnSnapshot,
                x.LabelArSnapshot,
                x.TypeSnapshot
            })
            .OrderByDescending(x => x.Count())
            .Take(MaxCustomInputsToReturn)
            .Select(inputGroup => new BranchDashboardCustomInputSegmentResponse
            {
                CustomInputId = inputGroup.Key.CustomInputId,
                LabelEn = inputGroup.Key.LabelEnSnapshot,
                LabelAr = inputGroup.Key.LabelArSnapshot,
                Type = inputGroup.Key.TypeSnapshot,
                TypeName = inputGroup.Key.TypeSnapshot.ToString(),

                Segments = inputGroup
                    .GroupBy(x => GetCustomInputValueText(x))
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                    .OrderByDescending(x => x.Count())
                    .Take(MaxSegmentsPerCustomInput)
                    .Select(valueGroup => new BranchDashboardCustomInputSegmentValueResponse
                    {
                        Value = valueGroup.Key,
                        ResponsesCount = valueGroup.Count(),
                        AverageScorePercentage = Round(valueGroup.Average(v => v.ScorePercentage)),
                        DetailsNavigation = BuildResponsesNavigation(
                            request,
                            period,
                            ("customInputId", inputGroup.Key.CustomInputId),
                            ("customInputType", inputGroup.Key.TypeSnapshot),
                            ("customInputValue", valueGroup.Key),
                            ("isScored", true))
                    })
                    .ToArray()
            })
            .Where(x => x.Segments.Count > 0)
            .ToArray();
    }

    private static IReadOnlyCollection<BranchDashboardCriticalResponseItem> BuildCriticalResponses(
        IReadOnlyCollection<DashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DashboardSurveyAnswerDto> answers,
        IReadOnlyCollection<DashboardCustomInputValueDto> customInputValues,
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

        var customInputsByResponseId = customInputValues
            .GroupBy(x => x.SurveyResponseId)
            .ToDictionary(
                x => x.Key,
                x => x
                    .Take(5)
                    .Select(value => new BranchDashboardCriticalResponseCustomInputItem
                    {
                        CustomInputId = value.CustomInputId,
                        LabelEnSnapshot = value.LabelEnSnapshot,
                        LabelArSnapshot = value.LabelArSnapshot,
                        Value = GetCustomInputValueText(value)
                    })
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

                return new BranchDashboardCriticalResponseItem
                {
                    SurveyResponseId = x.SurveyResponseId,
                    TemplateId = x.TemplateId,
                    TemplateNameEn = x.TemplateNameEn,
                    TemplateNameAr = x.TemplateNameAr,
                    SubmittedOnUtc = x.SubmittedOnUtc,
                    ScorePercentage = x.ScorePercentage,
                    ComplaintText = complaintText,
                    CustomInputs = customInputs ?? Array.Empty<BranchDashboardCriticalResponseCustomInputItem>(),
                    DetailsNavigation = DashboardDrillDownPathBuilder.Navigation(
                        "BranchResponseDetails",
                        $"/api/reports/branch-responses/{x.SurveyResponseId}")
                };
            })
            .ToArray();
    }

    private static string GetCustomInputValueText(DashboardCustomInputValueDto value)
    {
        return value.TypeSnapshot switch
        {
            TemplateCustomInputType.String => value.StringValue?.Trim() ?? string.Empty,
            TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };
    }

    private static DashboardDetailsNavigationResponse BuildResponsesNavigation(
        GetBranchDashboardQuery request,
        ResolvedDashboardPeriod period,
        params (string Name, object? Value)[] additionalFilters)
    {
        var filters = new List<(string Name, object? Value)>
        {
            ("scoreCalculationMode", request.ScoreCalculationMode),
            ("from", period.From),
            ("to", period.To),
            ("templateId", request.TemplateId)
        };

        foreach (var filter in additionalFilters)
        {
            filters.RemoveAll(existing =>
                string.Equals(existing.Name, filter.Name, StringComparison.OrdinalIgnoreCase));
            filters.Add(filter);
        }

        filters.Add(("pageNumber", 1));
        filters.Add(("pageSize", 10));

        return DashboardDrillDownPathBuilder.Navigation(
            "BranchResponses",
            "/api/reports/branch-responses",
            filters.ToArray());
    }


    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private sealed record ResolvedDashboardPeriod(
        DateOnly From,
        DateOnly To,
        bool IsDefaultPeriod);

    private sealed record PeriodResolveResult(
        ResolvedDashboardPeriod? Period,
        Error? Error)
    {
        public static PeriodResolveResult Ok(ResolvedDashboardPeriod period)
        {
            return new PeriodResolveResult(period, null);
        }

        public static PeriodResolveResult Fail(Error error)
        {
            return new PeriodResolveResult(null, error);
        }
    }
}
