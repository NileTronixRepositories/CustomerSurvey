using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
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

    private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
    private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyAnswer> _anonymousSurveyAnswerReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetAnonymousTemplateDashboardQueryHandler(
        IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
        IWriteReadRepository<BranchUser> branchUserReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
        IWriteReadRepository<AnonymousSurveyAnswer> anonymousSurveyAnswerReadRepository,
        IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> customInputValueReadRepository,
        ICurrentUser currentUser)
    {
        _branchAdminReadRepository = branchAdminReadRepository
            ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

        _branchUserReadRepository = branchUserReadRepository
            ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

        _anonymousTemplateReadRepository = anonymousTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

        _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseReadRepository));

        _anonymousSurveyAnswerReadRepository = anonymousSurveyAnswerReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousSurveyAnswerReadRepository));

        _customInputValueReadRepository = customInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

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

        var currentActor = await ResolveCurrentBranchActorAsync(
            _currentUser.UserId.Value,
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

    private async Task<CurrentBranchActorForAnonymousTemplateDashboardDto?> ResolveCurrentBranchActorAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchAdminForAnonymousTemplateDashboardSpec(applicationUserId),
            cancellationToken);

        if (branchAdmin is not null)
        {
            return branchAdmin;
        }

        return await _branchUserReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchUserForAnonymousTemplateDashboardSpec(applicationUserId),
            cancellationToken);
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

        var satisfiedResponses = scoredResponses.Count(x => x.ScorePercentage >= 80m);
        var neutralResponses = scoredResponses.Count(x => x.ScorePercentage >= 60m && x.ScorePercentage < 80m);
        var unhappyResponses = scoredResponses.Count(x => x.ScorePercentage < 60m);

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

            SatisfactionTrend = BuildTrend(
                scoredResponses,
                request.GroupBy),

            AnonymousTemplatePerformance = BuildAnonymousTemplatePerformance(
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

    private static IReadOnlyCollection<AnonymousTemplateDashboardTrendPointResponse> BuildTrend(
        IReadOnlyCollection<AnonymousDashboardSurveyResponseDto> scoredResponses,
        AnonymousTemplateDashboardGroupBy groupBy)
    {
        if (groupBy == AnonymousTemplateDashboardGroupBy.Month)
        {
            return scoredResponses
                .GroupBy(x => new
                {
                    x.SubmittedOnUtc.Year,
                    x.SubmittedOnUtc.Month
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(x => new AnonymousTemplateDashboardTrendPointResponse
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
            .Select(x => new AnonymousTemplateDashboardTrendPointResponse
            {
                Period = x.Key.ToString("yyyy-MM-dd"),
                ResponsesCount = x.Count(),
                AverageScorePercentage = Round(x.Average(r => r.ScorePercentage))
            })
            .ToArray();
    }

    private static IReadOnlyCollection<AnonymousTemplateDashboardPerformanceResponse> BuildAnonymousTemplatePerformance(
        IReadOnlyCollection<AnonymousDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<AnonymousDashboardSurveyAnswerDto> answers)
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
                x.TemplateNameEn,
                x.TemplateNameAr,
                x.Scope,
                x.Status,
                x.IsActive,
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
                    NameEn = x.Key.TemplateNameEn,
                    NameAr = x.Key.TemplateNameAr,
                    Scope = x.Key.Scope,
                    ScopeName = x.Key.Scope.ToString(),
                    Status = x.Key.Status,
                    StatusName = x.Key.Status.ToString(),
                    IsActive = x.Key.IsActive,
                    PublicUrl = x.Key.PublicUrl,
                    QrCode = x.Key.QrCode,
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

    private static IReadOnlyCollection<AnonymousTemplateDashboardQuestionInsightResponse> BuildLowestRatedQuestions(
        IReadOnlyCollection<AnonymousDashboardSurveyAnswerDto> answers,
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
                    AverageScorePercentage = Round(averageValue / 5m * 100m)
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.AnswersCount)
            .Take(topQuestionsCount)
            .ToArray();
    }

    private static IReadOnlyCollection<AnonymousTemplateDashboardCustomInputSegmentResponse> BuildCustomInputSegments(
        IReadOnlyCollection<AnonymousDashboardCustomInputValueDto> customInputValues)
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
                        AverageScorePercentage = Round(valueGroup.Average(v => v.ScorePercentage))
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
                    CustomInputs = customInputs ?? Array.Empty<AnonymousTemplateDashboardCriticalResponseCustomInputItem>()
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
