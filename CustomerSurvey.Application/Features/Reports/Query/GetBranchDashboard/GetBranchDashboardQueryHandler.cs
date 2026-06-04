using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
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
        ICurrentUser currentUser)
    {
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
            new GetBranchTemplatesForDashboardSpec(currentActor.BranchId),
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
        var scoredResponses = responses
            .Where(x => x.MaxScore > 0)
            .ToArray();

        var totalResponses = responses.Count;
        var scoredResponsesCount = scoredResponses.Length;
        var unscoredResponsesCount = totalResponses - scoredResponsesCount;

        var averageScore = scoredResponsesCount == 0
            ? 0m
            : Round(scoredResponses.Average(x => x.ScorePercentage));

        var satisfiedResponses = scoredResponses.Count(x => x.ScorePercentage >= 80m);
        var neutralResponses = scoredResponses.Count(x => x.ScorePercentage >= 60m && x.ScorePercentage < 80m);
        var unhappyResponses = scoredResponses.Count(x => x.ScorePercentage < 60m);

        var complaintsCount = answers.Count(x => x.QuestionType == QuestionType.Complain);
        var voiceAnswersCount = answers.Count(x => x.QuestionType == QuestionType.Voice);

        return new GetBranchDashboardResponse
        {
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

            SatisfactionTrend = BuildTrend(
                scoredResponses,
                request.GroupBy),

            TemplatePerformance = BuildTemplatePerformance(
                responses,
                answers),

            LowestRatedQuestions = BuildLowestRatedQuestions(
                answers,
                request.TopQuestionsCount),

            CustomInputSegments = BuildCustomInputSegments(
                customInputValues),

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
        BranchDashboardGroupBy groupBy)
    {
        if (groupBy == BranchDashboardGroupBy.Month)
        {
            return scoredResponses
                .GroupBy(x => new
                {
                    x.SubmittedOnUtc.Year,
                    x.SubmittedOnUtc.Month
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(x => new BranchDashboardTrendPointResponse
                {
                    Period = $"{x.Key.Year:D4}-{x.Key.Month:D2}",
                    ResponsesCount = x.Count(),
                    AverageScorePercentage = Round(x.Average(r => r.ScorePercentage))
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
                AverageScorePercentage = Round(x.Average(r => r.ScorePercentage))
            })
            .ToArray();
    }

    private static IReadOnlyCollection<BranchDashboardTemplatePerformanceResponse> BuildTemplatePerformance(
        IReadOnlyCollection<DashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DashboardSurveyAnswerDto> answers)
    {
        var complaintsCountByTemplateId = answers
            .Where(x => x.QuestionType == QuestionType.Complain)
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
                    RiskLevel = ResolveRiskLevel(average)
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.ResponsesCount)
            .ToArray();
    }

    private static IReadOnlyCollection<BranchDashboardQuestionInsightResponse> BuildLowestRatedQuestions(
        IReadOnlyCollection<DashboardSurveyAnswerDto> answers,
        int topQuestionsCount)
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
                x.Answer.TemplateId,
                x.Answer.TemplateNameEn,
                x.Answer.TemplateNameAr,
                x.Answer.QuestionId,
                x.Answer.QuestionTextEn,
                x.Answer.QuestionTextAr,
                x.Answer.QuestionType
            })
            .Select(x =>
            {
                var averageValue = Round(x.Average(a => (decimal)a.Value!.Value));
                return new BranchDashboardQuestionInsightResponse
                {
                    TemplateId = x.Key.TemplateId,
                    TemplateNameEn = x.Key.TemplateNameEn,
                    TemplateNameAr = x.Key.TemplateNameAr,
                    QuestionId = x.Key.QuestionId,
                    QuestionTextEn = x.Key.QuestionTextEn,
                    QuestionTextAr = x.Key.QuestionTextAr,
                    QuestionType = x.Key.QuestionType,
                    QuestionTypeName = x.Key.QuestionType.ToString(),
                    AnswersCount = x.Count(),
                    AverageValue = averageValue,
                    AverageScorePercentage = Round(averageValue / 5m * 100m)
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.AnswersCount)
            .Take(topQuestionsCount)
            .ToArray();
    }

    private static IReadOnlyCollection<BranchDashboardCustomInputSegmentResponse> BuildCustomInputSegments(
        IReadOnlyCollection<DashboardCustomInputValueDto> customInputValues)
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
            .Select(inputGroup => new BranchDashboardCustomInputSegmentResponse
            {
                CustomInputName = inputGroup.Key.NameSnapshot,
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
                        AverageScorePercentage = Round(valueGroup.Average(v => v.ScorePercentage))
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
                    CustomInputs = customInputs ?? Array.Empty<BranchDashboardCriticalResponseCustomInputItem>()
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
