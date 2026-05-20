using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
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
        ICurrentUser currentUser)
    {
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
            new GetDepartmentDashboardAssignedTemplatesSpec(currentDepartmentAdmin.DepartmentId),
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

        var satisfiedResponses = scoredResponses.Count(x => x.ScorePercentage >= 80m);
        var neutralResponses = scoredResponses.Count(x => x.ScorePercentage >= 60m && x.ScorePercentage < 80m);
        var unhappyResponses = scoredResponses.Count(x => x.ScorePercentage < 60m);

        var complaintsCount = answers.Count(x => x.QuestionType == QuestionType.Complain);
        var voiceAnswersCount = answers.Count(x => x.QuestionType == QuestionType.Voice);

        return new GetDepartmentDashboardResponse
        {
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

            SatisfactionTrend = BuildTrend(
                scoredResponses,
                request.GroupBy),

            OperatorPerformance = BuildOperatorPerformance(
                operators,
                responses,
                answers),

            TemplatePerformance = BuildTemplatePerformance(
                assignedTemplates,
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

    private static IReadOnlyCollection<DepartmentDashboardTrendPointResponse> BuildTrend(
        IReadOnlyCollection<DepartmentDashboardSurveyResponseDto> scoredResponses,
        DepartmentDashboardGroupBy groupBy)
    {
        if (groupBy == DepartmentDashboardGroupBy.Month)
        {
            return scoredResponses
                .GroupBy(x => new
                {
                    x.SubmittedOnUtc.Year,
                    x.SubmittedOnUtc.Month
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(x => new DepartmentDashboardTrendPointResponse
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
            .Select(x => new DepartmentDashboardTrendPointResponse
            {
                Period = x.Key.ToString("yyyy-MM-dd"),
                ResponsesCount = x.Count(),
                AverageScorePercentage = Round(x.Average(r => r.ScorePercentage))
            })
            .ToArray();
    }

    private static IReadOnlyCollection<DepartmentDashboardOperatorPerformanceResponse> BuildOperatorPerformance(
        IReadOnlyCollection<DepartmentDashboardOperatorDto> operators,
        IReadOnlyCollection<DepartmentDashboardSurveyResponseDto> responses,
        IReadOnlyCollection<DepartmentDashboardSurveyAnswerDto> answers)
    {
        var operatorsById = operators.ToDictionary(x => x.OperatorId);

        var responsesByOperatorId = responses
            .GroupBy(x => x.OperatorId)
            .ToDictionary(
                x => x.Key,
                x => x.ToArray());

        var complaintsCountByOperatorId = answers
            .Where(x => x.QuestionType == QuestionType.Complain)
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
                    RiskLevel = ResolveRiskLevel(average)
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
        IReadOnlyCollection<DepartmentDashboardSurveyAnswerDto> answers)
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
            .Where(x => x.QuestionType == QuestionType.Complain)
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
                    RiskLevel = ResolveRiskLevel(average)
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.ResponsesCount)
            .ThenBy(x => x.TemplateNameEn)
            .ToArray();
    }

    private static IReadOnlyCollection<DepartmentDashboardQuestionInsightResponse> BuildLowestRatedQuestions(
        IReadOnlyCollection<DepartmentDashboardSurveyAnswerDto> answers,
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
                return new DepartmentDashboardQuestionInsightResponse
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

    private static IReadOnlyCollection<DepartmentDashboardCustomInputSegmentResponse> BuildCustomInputSegments(
        IReadOnlyCollection<DepartmentDashboardCustomInputValueDto> customInputValues)
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
            .Select(inputGroup => new DepartmentDashboardCustomInputSegmentResponse
            {
                CustomInputName = inputGroup.Key.NameSnapshot,
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
                        AverageScorePercentage = Round(valueGroup.Average(v => v.ScorePercentage))
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
                    CustomInputs = customInputs ?? Array.Empty<DepartmentDashboardCriticalResponseCustomInputItem>()
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
