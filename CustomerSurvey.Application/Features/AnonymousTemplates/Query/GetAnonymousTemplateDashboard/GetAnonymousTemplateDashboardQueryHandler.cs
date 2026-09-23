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

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetAnonymousTemplateDashboardQueryHandler
    : IQueryHandler<GetAnonymousTemplateDashboardQuery, GetAnonymousTemplateDashboardResponse>
{
    private const int DefaultPeriodDays = 30;
    private const int MaxAllowedMonths = 12;
    private const int MaxCustomInputsToReturn = 5;
    private const int MaxSegmentsPerCustomInput = 10;

    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyAnswer> _anonymousSurveyAnswerReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
    private readonly ICurrentUser _currentUser;

    public GetAnonymousTemplateDashboardQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
        IWriteReadRepository<AnonymousSurveyAnswer> anonymousSurveyAnswerReadRepository,
        IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> customInputValueReadRepository,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        ICurrentUser currentUser)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _anonymousTemplateReadRepository = anonymousTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

        _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseReadRepository));

        _anonymousSurveyAnswerReadRepository = anonymousSurveyAnswerReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousSurveyAnswerReadRepository));

        _customInputValueReadRepository = customInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

        _currentBranchScopeResolver = currentBranchScopeResolver
            ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<GetAnonymousTemplateDashboardResponse>> Handle(
        GetAnonymousTemplateDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetAnonymousTemplateDashboardResponse>.Fail(new Error(
                Code: "AnonymousTemplates.Dashboard.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
            cancellationToken);

        if (currentBranchScope.IsFailure)
        {
            return Result<GetAnonymousTemplateDashboardResponse>.Fail(currentBranchScope.Errors);
        }

        var currentActor = await _branchReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchForAnonymousTemplateDashboardSpec(currentBranchScope.Value.BranchId),
            cancellationToken);

        if (currentActor is null)
        {
            return Result<GetAnonymousTemplateDashboardResponse>.Fail(new Error(
                Code: "AnonymousTemplates.Dashboard.CurrentBranchActorNotFound",
                Message: ErrorMessage.GetAnonymousTemplateResponsesPagination_CurrentActor_NotFound,
                Type: ErrorType.NotFound));
        }

        if (request.AnonymousTemplateId.HasValue)
        {
            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForDashboardSpec(
                    request.AnonymousTemplateId.Value,
                    currentActor.BranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<GetAnonymousTemplateDashboardResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Dashboard.TemplateNotFound",
                    Message: ErrorMessage.GetAnonymousTemplateResponsesPagination_Template_NotFound,
                    Type: ErrorType.NotFound));
            }
        }

        var periodResult = ResolvePeriod(request);

        if (periodResult.Error is not null)
        {
            return Result<GetAnonymousTemplateDashboardResponse>.Fail(periodResult.Error);
        }

        var period = periodResult.Period!;

        var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var anonymousTemplates = await _anonymousTemplateReadRepository.ListAsync(
            new GetBranchAnonymousTemplatesForDashboardSpec(currentActor.BranchId),
            cancellationToken);

        var responses = await _anonymousSurveyResponseReadRepository.ListAsync(
            new GetAnonymousDashboardSurveyResponsesSpec(
                currentActor.BranchId,
                fromUtc,
                toExclusiveUtc,
                request.AnonymousTemplateId),
            cancellationToken);

        var answers = await _anonymousSurveyAnswerReadRepository.ListAsync(
            new GetAnonymousDashboardSurveyAnswersSpec(
                currentActor.BranchId,
                fromUtc,
                toExclusiveUtc,
                request.AnonymousTemplateId),
            cancellationToken);

        var customInputValues = await _customInputValueReadRepository.ListAsync(
            new GetAnonymousDashboardCustomInputValuesSpec(
                currentActor.BranchId,
                fromUtc,
                toExclusiveUtc,
                request.AnonymousTemplateId),
            cancellationToken);

        var response = BuildResponse(
            request,
            currentActor,
            period,
            anonymousTemplates,
            responses,
            answers,
            customInputValues);

        return Result<GetAnonymousTemplateDashboardResponse>.Ok(response);
    }

    private static PeriodResolveResult ResolvePeriod(GetAnonymousTemplateDashboardQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-DefaultPeriodDays);

        if (from > to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "AnonymousTemplates.Dashboard.DateRangeInvalid",
                Message: ErrorMessage.GetBranchDashboard_DateRange_Invalid,
                Type: ErrorType.Validation));
        }

        if (from.AddMonths(MaxAllowedMonths) < to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "AnonymousTemplates.Dashboard.DateRangeTooLarge",
                Message: ErrorMessage.GetBranchDashboard_DateRange_TooLarge,
                Type: ErrorType.Validation));
        }

        return PeriodResolveResult.Ok(new ResolvedAnonymousTemplateDashboardPeriod(
            From: from,
            To: to,
            IsDefaultPeriod: !request.From.HasValue && !request.To.HasValue));
    }

    private static GetAnonymousTemplateDashboardResponse BuildResponse(
        GetAnonymousTemplateDashboardQuery request,
        CurrentBranchActorForAnonymousTemplateDashboardDto currentActor,
        ResolvedAnonymousTemplateDashboardPeriod period,
        IReadOnlyCollection<AnonymousTemplateDashboardItemDto> anonymousTemplates,
        IReadOnlyCollection<AnonymousDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<AnonymousDashboardSurveyAnswerDto> answers,
        IReadOnlyCollection<AnonymousDashboardCustomInputValueDto> customInputValues)
    {
        var scoredResponses = responses
            .Where(x => x.MaxScore > 0)
            .ToArray();

        var totalResponses = responses.Count;
        var scoredResponsesCount = scoredResponses.Length;
        var unscoredResponsesCount = totalResponses - scoredResponsesCount;

        var averageScore = scoredResponsesCount == 0
            ? 0m
            : Round(scoredResponses.Average(x => x.ScorePercentage));

        var satisfiedResponses = scoredResponses.Count(x => SatisfactionCategoryRule.Matches(
            x.MaxScore, x.ScorePercentage, SatisfactionCategory.Satisfied));
        var neutralResponses = scoredResponses.Count(x => SatisfactionCategoryRule.Matches(
            x.MaxScore, x.ScorePercentage, SatisfactionCategory.Neutral));
        var unhappyResponses = scoredResponses.Count(x => SatisfactionCategoryRule.Matches(
            x.MaxScore, x.ScorePercentage, SatisfactionCategory.Unhappy));

        var complaintsCount = answers.Count(x => x.QuestionType == QuestionType.Complain);
        var voiceAnswersCount = answers.Count(x => x.QuestionType == QuestionType.Voice);

        return new GetAnonymousTemplateDashboardResponse
        {
            Period = new AnonymousTemplateDashboardPeriodResponse
            {
                From = period.From,
                To = period.To,
                IsDefaultPeriod = period.IsDefaultPeriod,
                GroupBy = request.GroupBy.ToString()
            },

            Summary = new AnonymousTemplateDashboardSummaryResponse
            {
                BranchId = currentActor.BranchId,
                BranchNameEn = currentActor.BranchNameEn,
                BranchNameAr = currentActor.BranchNameAr,

                TotalAnonymousTemplates = anonymousTemplates.Count,
                ActiveAnonymousTemplates = anonymousTemplates.Count(x => x.IsActive),
                TemplatesWithResponsesCount = responses
                    .Select(x => x.AnonymousTemplateId)
                    .Distinct()
                    .Count(),

                TotalResponses = totalResponses,
                ScoredResponses = scoredResponsesCount,
                UnscoredResponses = unscoredResponsesCount,
                AverageScorePercentage = averageScore,

                SatisfiedResponses = satisfiedResponses,
                NeutralResponses = neutralResponses,
                UnhappyResponses = unhappyResponses,

                ComplaintsCount = complaintsCount,
                VoiceAnswersCount = voiceAnswersCount
            },

            Charts = new DashboardChartsResponse
            {
                SatisfactionDistribution = SatisfactionDistributionBuilder.Build(
                    scoredResponses,
                    x => x.ScorePercentage,
                    category => BuildAnonymousResponsesNavigation(
                        request,
                        period,
                        ("satisfactionCategory", category)))
            },

            SummaryActions = new DashboardSummaryActionsResponse
            {
                AllResponses = BuildAnonymousResponsesNavigation(request, period),
                Complaints = BuildAnonymousResponsesNavigation(request, period, ("hasComplaint", true)),
                VoiceAnswers = BuildAnonymousResponsesNavigation(request, period, ("hasVoice", true))
            },

            SatisfactionTrend = BuildTrend(
                scoredResponses,
                request,
                period),

            AnonymousTemplatePerformance = BuildAnonymousTemplatePerformance(
                responses,
                answers,
                request,
                period),

            LowestRatedQuestions = BuildLowestRatedQuestions(
                answers,
                request,
                period),

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

    private static IReadOnlyCollection<AnonymousTemplateDashboardTrendPointResponse> BuildTrend(
        IReadOnlyCollection<AnonymousDashboardSurveyResponseDto> scoredResponses,
        GetAnonymousTemplateDashboardQuery request,
        ResolvedAnonymousTemplateDashboardPeriod period)
    {
        if (request.GroupBy == AnonymousTemplateDashboardGroupBy.Month)
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
                    return new AnonymousTemplateDashboardTrendPointResponse
                    {
                        Period = periodKey,
                        ResponsesCount = x.Count(),
                        AverageScorePercentage = Round(x.Average(r => r.ScorePercentage)),
                        DetailsNavigation = BuildAnonymousResponsesNavigation(
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
            .Select(x => new AnonymousTemplateDashboardTrendPointResponse
            {
                Period = x.Key.ToString("yyyy-MM-dd"),
                ResponsesCount = x.Count(),
                AverageScorePercentage = Round(x.Average(r => r.ScorePercentage)),
                DetailsNavigation = BuildAnonymousResponsesNavigation(
                    request,
                    period,
                    ("from", x.Key),
                    ("to", x.Key),
                    ("isScored", true))
            })
            .ToArray();
    }

    private static IReadOnlyCollection<AnonymousTemplateDashboardPerformanceResponse> BuildAnonymousTemplatePerformance(
        IReadOnlyCollection<AnonymousDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<AnonymousDashboardSurveyAnswerDto> answers,
        GetAnonymousTemplateDashboardQuery request,
        ResolvedAnonymousTemplateDashboardPeriod period)
    {
        var complaintsCountByTemplateId = answers
            .Where(x => x.QuestionType == QuestionType.Complain)
            .GroupBy(x => x.AnonymousTemplateId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        return responses
            .GroupBy(x => new
            {
                x.AnonymousTemplateId,
                x.BranchId,
                x.BranchNameEn,
                x.BranchNameAr,
                x.TemplateNameEn,
                x.TemplateNameAr,
                x.Scope,
                x.IsActive,
                x.LogoPath,
                x.PublicUrl,
                x.QrCode
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
                    x.Key.AnonymousTemplateId,
                    out var complaintsCount);

                return new AnonymousTemplateDashboardPerformanceResponse
                {
                    AnonymousTemplateId = x.Key.AnonymousTemplateId,
                    BranchId = x.Key.BranchId,
                    BranchNameEn = x.Key.BranchNameEn,
                    BranchNameAr = x.Key.BranchNameAr,
                    NameEn = x.Key.TemplateNameEn,
                    NameAr = x.Key.TemplateNameAr,
                    Scope = x.Key.Scope,
                    ScopeName = x.Key.Scope.ToString(),
                    IsActive = x.Key.IsActive,
                    LogoPath = x.Key.LogoPath,
                    PublicUrl = x.Key.PublicUrl,
                    QrCode = x.Key.QrCode,
                    ResponsesCount = x.Count(),
                    ScoredResponsesCount = scored.Length,
                    AverageScorePercentage = average,
                    ComplaintsCount = complaintsCount,
                    RiskLevel = ResolveRiskLevel(average),
                    DetailsNavigation = BuildTemplateResponsesNavigation(
                        x.Key.AnonymousTemplateId,
                        request,
                        period)
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.ResponsesCount)
            .ToArray();
    }

    private static IReadOnlyCollection<AnonymousTemplateDashboardQuestionInsightResponse> BuildLowestRatedQuestions(
        IReadOnlyCollection<AnonymousDashboardSurveyAnswerDto> answers,
        GetAnonymousTemplateDashboardQuery request,
        ResolvedAnonymousTemplateDashboardPeriod period)
    {
        return answers
            .Where(x =>
                x.QuestionType == QuestionType.StarRating ||
                x.QuestionType == QuestionType.Smiles)
            .Select(x => new
            {
                Answer = x,
                Value = x.QuestionType == QuestionType.StarRating
                    ? x.StarRatingValue
                    : x.SmileValue
            })
            .Where(x => x.Value.HasValue)
            .GroupBy(x => new
            {
                x.Answer.AnonymousTemplateId,
                x.Answer.TemplateNameEn,
                x.Answer.TemplateNameAr,
                x.Answer.AnonymousTemplateQuestionId,
                x.Answer.QuestionId,
                x.Answer.QuestionTextEn,
                x.Answer.QuestionTextAr,
                x.Answer.QuestionType
            })
            .Select(x =>
            {
                var averageValue = Round(x.Average(a => (decimal)a.Value!.Value));

                return new AnonymousTemplateDashboardQuestionInsightResponse
                {
                    AnonymousTemplateId = x.Key.AnonymousTemplateId,
                    TemplateNameEn = x.Key.TemplateNameEn,
                    TemplateNameAr = x.Key.TemplateNameAr,
                    AnonymousTemplateQuestionId = x.Key.AnonymousTemplateQuestionId,
                    QuestionId = x.Key.QuestionId,
                    QuestionTextEn = x.Key.QuestionTextEn,
                    QuestionTextAr = x.Key.QuestionTextAr,
                    QuestionType = x.Key.QuestionType,
                    QuestionTypeName = x.Key.QuestionType.ToString(),
                    AnswersCount = x.Count(),
                    AverageValue = averageValue,
                    AverageScorePercentage = Round(averageValue / 5m * 100m),
                    DetailsNavigation = BuildTemplateResponsesNavigation(
                        x.Key.AnonymousTemplateId,
                        request,
                        period,
                        ("questionId", x.Key.QuestionId))
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.AnswersCount)
            .Take(request.TopQuestionsCount)
            .ToArray();
    }

    private static IReadOnlyCollection<AnonymousTemplateDashboardCustomInputSegmentResponse> BuildCustomInputSegments(
        IReadOnlyCollection<AnonymousDashboardCustomInputValueDto> customInputValues,
        GetAnonymousTemplateDashboardQuery request,
        ResolvedAnonymousTemplateDashboardPeriod period)
    {
        return customInputValues
            .Where(x => x.MaxScore > 0)
            .GroupBy(x => new
            {
                x.NameSnapshot,
                x.TypeSnapshot
            })
            .OrderByDescending(x => x.Count())
            .Take(MaxCustomInputsToReturn)
            .Select(inputGroup => new AnonymousTemplateDashboardCustomInputSegmentResponse
            {
                CustomInputName = inputGroup.Key.NameSnapshot,
                Type = inputGroup.Key.TypeSnapshot,
                TypeName = inputGroup.Key.TypeSnapshot.ToString(),

                Segments = inputGroup
                    .GroupBy(GetCustomInputValueText)
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                    .OrderByDescending(x => x.Count())
                    .Take(MaxSegmentsPerCustomInput)
                    .Select(valueGroup => new AnonymousTemplateDashboardCustomInputSegmentValueResponse
                    {
                        Value = valueGroup.Key,
                        ResponsesCount = valueGroup.Count(),
                        AverageScorePercentage = Round(valueGroup.Average(v => v.ScorePercentage)),
                        DetailsNavigation = BuildAnonymousResponsesNavigation(
                            request,
                            period,
                            ("customInputName", inputGroup.Key.NameSnapshot),
                            ("customInputType", inputGroup.Key.TypeSnapshot),
                            ("customInputValue", valueGroup.Key),
                            ("isScored", true))
                    })
                    .ToArray()
            })
            .Where(x => x.Segments.Count > 0)
            .ToArray();
    }

    private static IReadOnlyCollection<AnonymousTemplateDashboardCriticalResponseItem> BuildCriticalResponses(
        IReadOnlyCollection<AnonymousDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<AnonymousDashboardSurveyAnswerDto> answers,
        IReadOnlyCollection<AnonymousDashboardCustomInputValueDto> customInputValues,
        decimal criticalScoreThreshold,
        int criticalResponsesCount)
    {
        var complaintsByResponseId = answers
            .Where(x =>
                x.QuestionType == QuestionType.Complain &&
                !string.IsNullOrWhiteSpace(x.TextAnswer))
            .GroupBy(x => x.AnonymousSurveyResponseId)
            .ToDictionary(
                x => x.Key,
                x => x.First().TextAnswer);

        var customInputsByResponseId = customInputValues
            .GroupBy(x => x.AnonymousSurveyResponseId)
            .ToDictionary(
                x => x.Key,
                x => x
                    .Take(5)
                    .Select(value => new AnonymousTemplateDashboardCriticalResponseCustomInputItem
                    {
                        Name = value.NameSnapshot,
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
                    x.AnonymousSurveyResponseId,
                    out var complaintText);

                customInputsByResponseId.TryGetValue(
                    x.AnonymousSurveyResponseId,
                    out var customInputs);

                return new AnonymousTemplateDashboardCriticalResponseItem
                {
                    AnonymousSurveyResponseId = x.AnonymousSurveyResponseId,
                    AnonymousTemplateId = x.AnonymousTemplateId,
                    TemplateNameEn = x.TemplateNameEn,
                    TemplateNameAr = x.TemplateNameAr,
                    SubmittedOnUtc = x.SubmittedOnUtc,
                    ScorePercentage = x.ScorePercentage,
                    ComplaintText = complaintText,
                    CustomInputs = customInputs ?? Array.Empty<AnonymousTemplateDashboardCriticalResponseCustomInputItem>(),
                    DetailsNavigation = DashboardDrillDownPathBuilder.Navigation(
                        "AnonymousResponseDetails",
                        $"/api/anonymous-templates/{x.AnonymousTemplateId}/responses/{x.AnonymousSurveyResponseId}")
                };
            })
            .ToArray();
    }

    private static string GetCustomInputValueText(AnonymousDashboardCustomInputValueDto value)
    {
        return value.TypeSnapshot switch
        {
            TemplateCustomInputType.String => value.StringValue?.Trim() ?? string.Empty,
            TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };
    }

    private static DashboardDetailsNavigationResponse BuildAnonymousResponsesNavigation(
        GetAnonymousTemplateDashboardQuery request,
        ResolvedAnonymousTemplateDashboardPeriod period,
        params (string Name, object? Value)[] additionalFilters)
    {
        var filters = new List<(string Name, object? Value)>
        {
            ("from", period.From),
            ("to", period.To),
            ("anonymousTemplateId", request.AnonymousTemplateId)
        };

        AddOrReplace(filters, additionalFilters);
        filters.Add(("pageNumber", 1));
        filters.Add(("pageSize", 10));

        return DashboardDrillDownPathBuilder.Navigation(
            "AnonymousResponses",
            "/api/reports/anonymous-responses",
            filters.ToArray());
    }

    private static DashboardDetailsNavigationResponse BuildTemplateResponsesNavigation(
        Guid anonymousTemplateId,
        GetAnonymousTemplateDashboardQuery request,
        ResolvedAnonymousTemplateDashboardPeriod period,
        params (string Name, object? Value)[] additionalFilters)
    {
        var filters = new List<(string Name, object? Value)>
        {
            ("fromDate", period.From),
            ("toDate", period.To.AddDays(1))
        };

        AddOrReplace(filters, additionalFilters);
        filters.Add(("pageNumber", 1));
        filters.Add(("pageSize", 10));

        return DashboardDrillDownPathBuilder.Navigation(
            "AnonymousTemplateResponses",
            $"/api/anonymous-templates/{anonymousTemplateId}/responses",
            filters.ToArray());
    }

    private static void AddOrReplace(
        List<(string Name, object? Value)> filters,
        IReadOnlyCollection<(string Name, object? Value)> additionalFilters)
    {
        foreach (var filter in additionalFilters)
        {
            filters.RemoveAll(existing => string.Equals(existing.Name, filter.Name, StringComparison.OrdinalIgnoreCase));
            filters.Add(filter);
        }
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

    private sealed record ResolvedAnonymousTemplateDashboardPeriod(
        DateOnly From,
        DateOnly To,
        bool IsDefaultPeriod);

    private sealed record PeriodResolveResult(
        ResolvedAnonymousTemplateDashboardPeriod? Period,
        Error? Error)
    {
        public static PeriodResolveResult Ok(ResolvedAnonymousTemplateDashboardPeriod period)
        {
            return new PeriodResolveResult(period, null);
        }

        public static PeriodResolveResult Fail(Error error)
        {
            return new PeriodResolveResult(null, error);
        }
    }
}
