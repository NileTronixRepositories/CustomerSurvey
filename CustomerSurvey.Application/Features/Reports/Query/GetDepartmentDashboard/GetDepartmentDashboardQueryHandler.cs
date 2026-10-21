using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

internal sealed class GetDepartmentDashboardQueryHandler
    : IQueryHandler<GetDepartmentDashboardQuery, GetDepartmentDashboardResponse>
{
    private const int DefaultPeriodDays = 30;
    private const int MaxAllowedMonths = 12;
    private const int MaxCustomInputsToReturn = 5;
    private const int MaxSegmentsPerCustomInput = 10;

    private readonly CustomerSurvey.Application.Abstraction.Reports.ITemplateAvailabilityReader? _availabilityReader;
    private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
    private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
    private readonly IWriteReadRepository<OperatorTemplate> _operatorTemplateReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDepartmentDashboardQueryHandler(
        IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
        IWriteReadRepository<DomainOperator> operatorReadRepository,
        IWriteReadRepository<OperatorTemplate> operatorTemplateReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        ICurrentUser currentUser,
        CustomerSurvey.Application.Abstraction.Reports.ITemplateAvailabilityReader? availabilityReader = null)
    {
        _availabilityReader = availabilityReader;
        _departmentAdminReadRepository = departmentAdminReadRepository
            ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

        _operatorReadRepository = operatorReadRepository
            ?? throw new ArgumentNullException(nameof(operatorReadRepository));

        _operatorTemplateReadRepository = operatorTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(operatorTemplateReadRepository));

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

    public async Task<Result<GetDepartmentDashboardResponse>> Handle(
        GetDepartmentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetDepartmentDashboardResponse>.Fail(new Error(
                Code: "Reports.DepartmentDashboard.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentDepartmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
            new GetCurrentDepartmentAdminForDepartmentDashboardSpec(_currentUser.UserId.Value),
            cancellationToken);

        if (currentDepartmentAdmin is null)
        {
            return Result<GetDepartmentDashboardResponse>.Fail(new Error(
                Code: "Reports.DepartmentDashboard.CurrentDepartmentAdminNotFound",
                Message: ErrorMessage.GetDepartmentDashboard_CurrentDepartmentAdmin_NotFound,
                Type: ErrorType.NotFound));
        }

        if (request.TemplateId.HasValue)
        {
            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForDepartmentDashboardSpec(request.TemplateId.Value),
                cancellationToken);

            if (template is null)
            {
                return Result<GetDepartmentDashboardResponse>.Fail(new Error(
                    Code: "Reports.DepartmentDashboard.TemplateNotFound",
                    Message: ErrorMessage.GetDepartmentDashboard_Template_NotFound,
                    Type: ErrorType.NotFound));
            }
        }

        var periodResult = ResolvePeriod(request);

        if (periodResult.Error is not null)
        {
            return Result<GetDepartmentDashboardResponse>.Fail(periodResult.Error);
        }

        var period = periodResult.Period!;

        var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var operators = await _operatorReadRepository.ListAsync(
            new GetDepartmentDashboardOperatorsSpec(currentDepartmentAdmin.DepartmentId),
            cancellationToken);

        var assignedTemplates = await _operatorTemplateReadRepository.ListAsync(
            new GetDepartmentDashboardAssignedTemplatesSpec(currentDepartmentAdmin.DepartmentId, request.TemplateId),
            cancellationToken);

        var responses = await _surveyResponseReadRepository.ListAsync(
            new GetDepartmentDashboardSurveyResponsesSpec(
                currentDepartmentAdmin.DepartmentId,
                fromUtc,
                toExclusiveUtc,
                request.TemplateId),
            cancellationToken);

        var answers = await _surveyAnswerReadRepository.ListAsync(
            new GetDepartmentDashboardSurveyAnswersSpec(
                currentDepartmentAdmin.DepartmentId,
                fromUtc,
                toExclusiveUtc,
                request.TemplateId),
            cancellationToken);

        var customInputValues = await _customInputValueReadRepository.ListAsync(
            new GetDepartmentDashboardCustomInputValuesSpec(
                currentDepartmentAdmin.DepartmentId,
                fromUtc,
                toExclusiveUtc,
                request.TemplateId),
            cancellationToken);

        var response = BuildResponse(
            request,
            currentDepartmentAdmin,
            period,
            operators,
            assignedTemplates,
            responses,
            answers,
            customInputValues);

        if (_availabilityReader is not null)
        {
            var historicalTemplateIds = await _surveyResponseReadRepository.ListAsync(
                new DepartmentHistoricalTemplateIdsSpec(currentDepartmentAdmin.DepartmentId), cancellationToken);
            response = response with { Availability = await _availabilityReader.ReadTemplatesAsync(
                assignedTemplates.Select(x => x.TemplateId).Concat(historicalTemplateIds)
                    .Where(x => !request.TemplateId.HasValue || x == request.TemplateId), fromUtc, toExclusiveUtc, cancellationToken) };
        }

        response = response with { Availability = response.Availability with { TemplatesWithResponsesCount = responses.Select(x => x.TemplateId).Distinct().Count() } };

        return Result<GetDepartmentDashboardResponse>.Ok(response);
    }

    private static PeriodResolveResult ResolvePeriod(GetDepartmentDashboardQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-DefaultPeriodDays);

        if (from > to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.DepartmentDashboard.DateRangeInvalid",
                Message: ErrorMessage.GetDepartmentDashboard_DateRange_Invalid,
                Type: ErrorType.Validation));
        }

        if (from.AddMonths(MaxAllowedMonths) < to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.DepartmentDashboard.DateRangeTooLarge",
                Message: ErrorMessage.GetDepartmentDashboard_DateRange_TooLarge,
                Type: ErrorType.Validation));
        }

        return PeriodResolveResult.Ok(new ResolvedDepartmentDashboardPeriod(
            From: from,
            To: to,
            IsDefaultPeriod: !request.From.HasValue && !request.To.HasValue));
    }

    private static GetDepartmentDashboardResponse BuildResponse(
        GetDepartmentDashboardQuery request,
        CurrentDepartmentAdminForDepartmentDashboardDto currentDepartmentAdmin,
        ResolvedDepartmentDashboardPeriod period,
        IReadOnlyCollection<DepartmentDashboardOperatorDto> operators,
        IReadOnlyCollection<DepartmentDashboardAssignedTemplateDto> assignedTemplateAssignments,
        IReadOnlyCollection<DepartmentDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DepartmentDashboardSurveyAnswerDto> answers,
        IReadOnlyCollection<DepartmentDashboardCustomInputValueDto> customInputValues)
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

        var assignedTemplates = assignedTemplateAssignments
            .GroupBy(x => x.TemplateId)
            .Select(x => x.First())
            .ToArray();

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
            x.MaxScore, x.ScorePercentage, SatisfactionCategory.Satisfied, x.Criteria));
        var neutralResponses = scoredResponses.Count(x => SatisfactionCategoryRule.Matches(
            x.MaxScore, x.ScorePercentage, SatisfactionCategory.Neutral, x.Criteria));
        var unhappyResponses = scoredResponses.Count(x => SatisfactionCategoryRule.Matches(
            x.MaxScore, x.ScorePercentage, SatisfactionCategory.Unhappy, x.Criteria));

        var complaintsCount = answers.Count(x =>
            x.QuestionType == QuestionType.Complain &&
            !string.IsNullOrWhiteSpace(x.TextAnswer));
        var voiceAnswersCount = answers.Count(x => x.QuestionType == QuestionType.Voice);

        return new GetDepartmentDashboardResponse
        {
            HasMixedClassificationCriteria = responses.Where(x => x.MaxScore > 0).Select(x => x.Criteria).Distinct().Skip(1).Any(),
            ScoreCalculationMode = request.ScoreCalculationMode,
            StoredBaselineResponsesCount = responses.Count(x => x.ScoreDerivation == "StoredRootBaseline"),
            RequestedModeAvailable = responses.All(x => x.RequestedModeAvailable),
            Period = new DepartmentDashboardPeriodResponse
            {
                From = period.From,
                To = period.To,
                IsDefaultPeriod = period.IsDefaultPeriod,
                GroupBy = request.GroupBy.ToString()
            },

            Summary = new DepartmentDashboardSummaryResponse
            {
                DepartmentId = currentDepartmentAdmin.DepartmentId,
                DepartmentNameEn = currentDepartmentAdmin.DepartmentNameEn,
                DepartmentNameAr = currentDepartmentAdmin.DepartmentNameAr,

                TotalOperators = operators.Count,
                ActiveOperators = operators.Count(x => x.IsActive),
                TotalAssignedTemplates = assignedTemplates.Length,
                ActiveAssignedTemplates = assignedTemplates.Count(x => x.IsActive),
                TemplatesWithResponsesCount = responses
                    .Select(x => x.TemplateId)
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
                    category => BuildDepartmentResponsesNavigation(
                        request,
                        period,
                        ("satisfactionCategory", category)))
            },

            SummaryActions = new DashboardSummaryActionsResponse
            {
                ScoredResponses = BuildDepartmentResponsesNavigation(request, period, ("isScored", true)),
                AllResponses = BuildDepartmentResponsesNavigation(request, period),
                Complaints = BuildDepartmentResponsesNavigation(request, period, ("hasComplaint", true), ("complaintItemsOnly", true)),
                VoiceAnswers = BuildDepartmentResponsesNavigation(request, period, ("hasVoice", true))
            },

            SatisfactionTrend = BuildTrend(
                scoredResponses,
                request,
                period),

            OperatorPerformance = BuildOperatorPerformance(
                operators,
                responses,
                answers,
                request,
                period),

            TemplatePerformance = BuildTemplatePerformance(
                assignedTemplates,
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

    private static IReadOnlyCollection<DepartmentDashboardTrendPointResponse> BuildTrend(
        IReadOnlyCollection<DepartmentDashboardSurveyResponseDto> scoredResponses,
        GetDepartmentDashboardQuery request,
        ResolvedDepartmentDashboardPeriod period)
    {
        if (request.GroupBy == DepartmentDashboardGroupBy.Month)
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
                    return new DepartmentDashboardTrendPointResponse
                    {
                        Period = periodKey,
                        ResponsesCount = x.Count(),
                        AverageScorePercentage = Round(x.Average(r => r.ScorePercentage)),
                        DetailsNavigation = BuildDepartmentResponsesNavigation(
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
            .Select(x => new DepartmentDashboardTrendPointResponse
            {
                Period = x.Key.ToString("yyyy-MM-dd"),
                ResponsesCount = x.Count(),
                AverageScorePercentage = Round(x.Average(r => r.ScorePercentage)),
                DetailsNavigation = BuildDepartmentResponsesNavigation(
                    request,
                    period,
                    ("from", x.Key),
                    ("to", x.Key),
                    ("isScored", true))
            })
            .ToArray();
    }

    private static IReadOnlyCollection<DepartmentDashboardOperatorPerformanceResponse> BuildOperatorPerformance(
        IReadOnlyCollection<DepartmentDashboardOperatorDto> operators,
        IReadOnlyCollection<DepartmentDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DepartmentDashboardSurveyAnswerDto> answers,
        GetDepartmentDashboardQuery request,
        ResolvedDepartmentDashboardPeriod period)
    {
        var operatorsById = operators.ToDictionary(x => x.OperatorId);

        var responsesByOperatorId = responses
            .GroupBy(x => x.OperatorId)
            .ToDictionary(
                x => x.Key,
                x => x.ToArray());

        var complaintsCountByOperatorId = answers
            .Where(x =>
                x.QuestionType == QuestionType.Complain &&
                !string.IsNullOrWhiteSpace(x.TextAnswer))
            .GroupBy(x => x.OperatorId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        var voiceCountByOperatorId = answers
            .Where(x => x.QuestionType == QuestionType.Voice)
            .GroupBy(x => x.OperatorId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        var operatorIds = operators
            .Select(x => x.OperatorId)
            .Concat(responses.Select(x => x.OperatorId))
            .Distinct()
            .ToArray();

        return operatorIds
            .Select(operatorId =>
            {
                operatorsById.TryGetValue(operatorId, out var operatorProfile);
                responsesByOperatorId.TryGetValue(operatorId, out var operatorResponses);

                operatorResponses ??= Array.Empty<DepartmentDashboardSurveyResponseDto>();

                var scored = operatorResponses
                    .Where(x => x.MaxScore > 0)
                    .ToArray();

                var average = scored.Length == 0
                    ? 0m
                    : Round(scored.Average(x => x.ScorePercentage));

                var firstResponse = operatorResponses.FirstOrDefault();

                complaintsCountByOperatorId.TryGetValue(operatorId, out var complaintsCount);
                voiceCountByOperatorId.TryGetValue(operatorId, out var voiceAnswersCount);

                return new DepartmentDashboardOperatorPerformanceResponse
                {
                    OperatorId = operatorId,
                    OperatorNameEn = operatorProfile?.OperatorNameEn ?? firstResponse?.OperatorNameEn ?? string.Empty,
                    OperatorNameAr = operatorProfile?.OperatorNameAr ?? firstResponse?.OperatorNameAr,
                    IsActive = operatorProfile?.IsActive ?? true,
                    ResponsesCount = operatorResponses.Length,
                    ScoredResponsesCount = scored.Length,
                    AverageScorePercentage = average,
                    ComplaintsCount = complaintsCount,
                    VoiceAnswersCount = voiceAnswersCount,
                    LastResponseOnUtc = operatorResponses.Length == 0
                        ? null
                        : operatorResponses.Max(x => x.SubmittedOnUtc),
                    RiskLevel = CustomerSurvey.Application.Features.Reports.Shared.EvaluationCriteria.Combine(scored.Select(x => x.Criteria)).Risk(scored.Length == 0 ? null : average),
                    DetailsNavigation = BuildOperatorResponsesNavigation(
                        operatorId,
                        request,
                        period)
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.ResponsesCount)
            .ThenBy(x => x.OperatorNameEn)
            .ToArray();
    }

    private static IReadOnlyCollection<DepartmentDashboardTemplatePerformanceResponse> BuildTemplatePerformance(
        IReadOnlyCollection<DepartmentDashboardAssignedTemplateDto> assignedTemplates,
        IReadOnlyCollection<DepartmentDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DepartmentDashboardSurveyAnswerDto> answers,
        GetDepartmentDashboardQuery request,
        ResolvedDepartmentDashboardPeriod period)
    {
        var assignedTemplatesById = assignedTemplates
            .GroupBy(x => x.TemplateId)
            .ToDictionary(
                x => x.Key,
                x => x.First());

        var responsesByTemplateId = responses
            .GroupBy(x => x.TemplateId)
            .ToDictionary(
                x => x.Key,
                x => x.ToArray());

        var complaintsCountByTemplateId = answers
            .Where(x =>
                x.QuestionType == QuestionType.Complain &&
                !string.IsNullOrWhiteSpace(x.TextAnswer))
            .GroupBy(x => x.TemplateId)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        var templateIds = assignedTemplatesById.Keys
            .Concat(responsesByTemplateId.Keys)
            .Distinct()
            .ToArray();

        return templateIds
            .Select(templateId =>
            {
                assignedTemplatesById.TryGetValue(templateId, out var assignedTemplate);
                responsesByTemplateId.TryGetValue(templateId, out var templateResponses);

                templateResponses ??= Array.Empty<DepartmentDashboardSurveyResponseDto>();

                var firstResponse = templateResponses.FirstOrDefault();

                var scored = templateResponses
                    .Where(r => r.MaxScore > 0)
                    .ToArray();

                var average = scored.Length == 0
                    ? 0m
                    : Round(scored.Average(r => r.ScorePercentage));

                complaintsCountByTemplateId.TryGetValue(
                    templateId,
                    out var complaintsCount);

                return new DepartmentDashboardTemplatePerformanceResponse
                {
                    TemplateId = templateId,
                    TemplateNameEn = assignedTemplate?.TemplateNameEn ?? firstResponse?.TemplateNameEn ?? string.Empty,
                    TemplateNameAr = assignedTemplate?.TemplateNameAr ?? firstResponse?.TemplateNameAr,
                    BranchId = assignedTemplate?.BranchId ?? firstResponse?.BranchId ?? Guid.Empty,
                    BranchNameEn = assignedTemplate?.BranchNameEn ?? firstResponse?.BranchNameEn ?? string.Empty,
                    BranchNameAr = assignedTemplate?.BranchNameAr ?? firstResponse?.BranchNameAr,
                    IsActive = assignedTemplate?.IsActive ?? true,
                    ResponsesCount = templateResponses.Length,
                    ScoredResponsesCount = scored.Length,
                    AverageScorePercentage = average,
                    ComplaintsCount = complaintsCount,
                    RiskLevel = CustomerSurvey.Application.Features.Reports.Shared.EvaluationCriteria.Combine(scored.Select(x => x.Criteria)).Risk(scored.Length == 0 ? null : average),
                    DetailsNavigation = BuildDepartmentResponsesNavigation(
                        request,
                        period,
                        ("templateId", templateId))
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.ResponsesCount)
            .ThenBy(x => x.TemplateNameEn)
            .ToArray();
    }

    private static IReadOnlyCollection<DepartmentDashboardQuestionInsightResponse> BuildLowestRatedQuestions(
        IReadOnlyCollection<DepartmentDashboardSurveyAnswerDto> answers,
        GetDepartmentDashboardQuery request,
        ResolvedDepartmentDashboardPeriod period, bool best = false)
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
                return new DepartmentDashboardQuestionInsightResponse
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
                    DetailsNavigation = BuildDepartmentResponsesNavigation(
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

    private static IReadOnlyCollection<DepartmentDashboardCustomInputSegmentResponse> BuildCustomInputSegments(
        IReadOnlyCollection<DepartmentDashboardCustomInputValueDto> customInputValues,
        GetDepartmentDashboardQuery request,
        ResolvedDepartmentDashboardPeriod period)
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
            .Select(inputGroup => new DepartmentDashboardCustomInputSegmentResponse
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
                    .Select(valueGroup => new DepartmentDashboardCustomInputSegmentValueResponse
                    {
                        Value = valueGroup.Key,
                        ResponsesCount = valueGroup.Count(),
                        AverageScorePercentage = Round(valueGroup.Average(v => v.ScorePercentage)),
                        DetailsNavigation = BuildDepartmentResponsesNavigation(
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

    private static IReadOnlyCollection<DepartmentDashboardCriticalResponseItem> BuildCriticalResponses(
        IReadOnlyCollection<DepartmentDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DepartmentDashboardSurveyAnswerDto> answers,
        IReadOnlyCollection<DepartmentDashboardCustomInputValueDto> customInputValues,
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
                    .Select(value => new DepartmentDashboardCriticalResponseCustomInputItem
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

                return new DepartmentDashboardCriticalResponseItem
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
                    CustomInputs = customInputs ?? Array.Empty<DepartmentDashboardCriticalResponseCustomInputItem>(),
                    DetailsNavigation = DashboardDrillDownPathBuilder.Navigation(
                        "DepartmentOperatorResponseDetails",
                        $"/api/reports/department-operators/{x.OperatorId}/responses/{x.SurveyResponseId}")
                };
            })
            .ToArray();
    }

    private static string GetCustomInputValueText(DepartmentDashboardCustomInputValueDto value)
    {
        return value.TypeSnapshot switch
        {
            TemplateCustomInputType.String => value.StringValue?.Trim() ?? string.Empty,
            TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };
    }

    private static DashboardDetailsNavigationResponse BuildDepartmentResponsesNavigation(
        GetDepartmentDashboardQuery request,
        ResolvedDepartmentDashboardPeriod period,
        params (string Name, object? Value)[] additionalFilters)
        => BuildResponsesNavigation(
            "DepartmentResponses",
            "/api/reports/department-responses",
            request,
            period,
            additionalFilters);

    private static DashboardDetailsNavigationResponse BuildOperatorResponsesNavigation(
        Guid operatorId,
        GetDepartmentDashboardQuery request,
        ResolvedDepartmentDashboardPeriod period)
        => BuildResponsesNavigation(
            "DepartmentOperatorResponses",
            $"/api/reports/department-operators/{operatorId}/responses",
            request,
            period,
            Array.Empty<(string Name, object? Value)>());

    private static DashboardDetailsNavigationResponse BuildResponsesNavigation(
        string routeType,
        string path,
        GetDepartmentDashboardQuery request,
        ResolvedDepartmentDashboardPeriod period,
        IReadOnlyCollection<(string Name, object? Value)> additionalFilters)
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
            filters.RemoveAll(existing => string.Equals(existing.Name, filter.Name, StringComparison.OrdinalIgnoreCase));
            filters.Add(filter);
        }

        filters.Add(("pageNumber", 1));
        filters.Add(("pageSize", 10));
        return DashboardDrillDownPathBuilder.Navigation(routeType, path, filters.ToArray());
    }


    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private sealed record ResolvedDepartmentDashboardPeriod(
        DateOnly From,
        DateOnly To,
        bool IsDefaultPeriod);

    private sealed record PeriodResolveResult(
        ResolvedDepartmentDashboardPeriod? Period,
        Error? Error)
    {
        public static PeriodResolveResult Ok(ResolvedDepartmentDashboardPeriod period)
        {
            return new PeriodResolveResult(period, null);
        }

        public static PeriodResolveResult Fail(Error error)
        {
            return new PeriodResolveResult(null, error);
        }
    }
}
