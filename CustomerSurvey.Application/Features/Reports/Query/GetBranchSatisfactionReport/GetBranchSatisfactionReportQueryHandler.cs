using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed class GetBranchSatisfactionReportQueryHandler
        : IQueryHandler<GetBranchSatisfactionReportQuery, GetBranchSatisfactionReportResponse>
    {
        private const int MaxAllowedMonths = 12;

        private const string PeriodSourceUserProvided = "UserProvided";
        private const string PeriodSourceLastSixMonths = "LastSixMonths";
        private const string PeriodSourceTemplateCreatedOn = "TemplateCreatedOn";

        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
        private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetBranchSatisfactionReportQueryHandler(
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
            IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
            ICurrentUser currentUser)
        {
            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _surveyResponseReadRepository = surveyResponseReadRepository
                ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));

            _surveyAnswerReadRepository = surveyAnswerReadRepository
                ?? throw new ArgumentNullException(nameof(surveyAnswerReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetBranchSatisfactionReportResponse>> Handle(
            GetBranchSatisfactionReportQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetBranchSatisfactionReportResponse>.Fail(new Error(
                    Code: "Reports.BranchSatisfaction.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentActor = await ResolveCurrentBranchActorAsync(
                currentApplicationUserId,
                cancellationToken);

            if (currentActor is null)
            {
                return Result<GetBranchSatisfactionReportResponse>.Fail(new Error(
                    Code: "Reports.BranchSatisfaction.CurrentBranchActorNotFound",
                    Message: ErrorMessage.GetBranchSatisfactionReport_CurrentBranchActor_NotFound,
                    Type: ErrorType.NotFound));
            }

            TemplateForBranchSatisfactionReportDto? template = null;

            if (request.TemplateId.HasValue)
            {
                template = await _templateReadRepository.FirstOrDefaultAsync(
                    new GetTemplateForBranchSatisfactionReportSpec(
                        request.TemplateId.Value,
                        currentActor.BranchId),
                    cancellationToken);

                if (template is null)
                {
                    return Result<GetBranchSatisfactionReportResponse>.Fail(new Error(
                        Code: "Reports.BranchSatisfaction.TemplateNotFound",
                        Message: ErrorMessage.GetBranchSatisfactionReport_Template_NotFound,
                        Type: ErrorType.NotFound));
                }
            }

            var periodResult = ResolvePeriod(request, template);

            if (periodResult.IsFailure)
            {
                return Result<GetBranchSatisfactionReportResponse>.Fail(periodResult.Errors);
            }

            var period = periodResult.Value;

            var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
            var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var responses = await _surveyResponseReadRepository.ListAsync(
                new GetSatisfactionSurveyResponsesForBranchReportSpec(
                    branchId: currentActor.BranchId,
                    fromUtc: fromUtc,
                    toExclusiveUtc: toExclusiveUtc,
                    templateId: request.TemplateId),
                cancellationToken);

            var answers = await _surveyAnswerReadRepository.ListAsync(
                new GetSatisfactionAnswersForBranchReportSpec(
                    branchId: currentActor.BranchId,
                    fromUtc: fromUtc,
                    toExclusiveUtc: toExclusiveUtc,
                    templateId: request.TemplateId),
                cancellationToken);

            var response = BuildResponse(
                period: period,
                responses: responses,
                answers: answers);

            return Result<GetBranchSatisfactionReportResponse>.Ok(response);
        }

        private async Task<CurrentBranchActorForBranchSatisfactionReportDto?> ResolveCurrentBranchActorAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForBranchSatisfactionReportSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForBranchSatisfactionReportSpec(applicationUserId),
                cancellationToken);

            return branchUser;
        }

        private static Result<ResolvedSatisfactionPeriod> ResolvePeriod(
            GetBranchSatisfactionReportQuery request,
            TemplateForBranchSatisfactionReportDto? template)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            DateOnly from;
            DateOnly to;
            bool isDefaultPeriod;
            string periodSource;

            if (request.From.HasValue || request.To.HasValue)
            {
                to = request.To ?? today;
                from = request.From ?? to.AddMonths(-6);

                isDefaultPeriod = false;
                periodSource = PeriodSourceUserProvided;
            }
            else
            {
                to = today;

                var lastSixMonthsFrom = today.AddMonths(-6);

                if (template is not null)
                {
                    var templateCreatedOn = DateOnly.FromDateTime(template.CreatedOnUtc);

                    if (templateCreatedOn > lastSixMonthsFrom)
                    {
                        from = templateCreatedOn;
                        periodSource = PeriodSourceTemplateCreatedOn;
                    }
                    else
                    {
                        from = lastSixMonthsFrom;
                        periodSource = PeriodSourceLastSixMonths;
                    }
                }
                else
                {
                    from = lastSixMonthsFrom;
                    periodSource = PeriodSourceLastSixMonths;
                }

                isDefaultPeriod = true;
            }

            if (from > to)
            {
                return Result<ResolvedSatisfactionPeriod>.Fail(new Error(
                    Code: "Reports.BranchSatisfaction.DateRangeInvalid",
                    Message: ErrorMessage.GetBranchSatisfactionReport_DateRange_Invalid,
                    Type: ErrorType.Validation));
            }

            if (from < to.AddMonths(-MaxAllowedMonths))
            {
                return Result<ResolvedSatisfactionPeriod>.Fail(new Error(
                    Code: "Reports.BranchSatisfaction.DateRangeMaxExceeded",
                    Message: ErrorMessage.GetBranchSatisfactionReport_DateRange_MaxExceeded,
                    Type: ErrorType.Validation));
            }

            var period = new ResolvedSatisfactionPeriod(
                From: from,
                To: to,
                IsDefaultPeriod: isDefaultPeriod,
                PeriodSource: periodSource);

            return Result<ResolvedSatisfactionPeriod>.Ok(period);
        }

        private static GetBranchSatisfactionReportResponse BuildResponse(
            ResolvedSatisfactionPeriod period,
            IReadOnlyCollection<SatisfactionSurveyResponseFlatDto> responses,
            IReadOnlyCollection<SatisfactionAnswerFlatDto> answers)
        {
            var scoredResponses = responses
                .Where(x => x.MaxScore > 0)
                .ToArray();

            var totalResponses = responses.Count;
            var scoredResponsesCount = scoredResponses.Length;
            var unscoredResponses = Math.Max(0, totalResponses - scoredResponsesCount);

            var overallScore = scoredResponses.Length == 0
                ? 0m
                : Round(scoredResponses.Average(x => x.ScorePercentage));

            var satisfiedResponses = scoredResponses.Count(x => x.ScorePercentage >= 80m);
            var neutralResponses = scoredResponses.Count(x => x.ScorePercentage >= 60m && x.ScorePercentage < 80m);
            var unsatisfiedResponses = scoredResponses.Count(x => x.ScorePercentage < 60m);

            var complaintsCount = answers.Count(x => x.QuestionType == QuestionType.Complain);
            var voiceAnswersCount = answers.Count(x => x.QuestionType == QuestionType.Voice);

            var distributionValues = scoredResponses
                .Select(x => ToDistributionValue(x.ScorePercentage))
                .ToArray();

            var distribution = BuildDistribution(distributionValues);

            var byTemplate = scoredResponses
                .GroupBy(x => new
                {
                    x.TemplateId,
                    x.TemplateNameEn,
                    x.TemplateNameAr
                })
                .Select(x => new SatisfactionByTemplateItemResponse
                {
                    TemplateId = x.Key.TemplateId,
                    TemplateNameEn = x.Key.TemplateNameEn,
                    TemplateNameAr = x.Key.TemplateNameAr,
                    Score = Round(x.Average(response => response.ScorePercentage)),
                    ResponsesCount = x.Count(),
                    ScoredAnswersCount = x.Sum(response => response.MaxScore / 5)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.TemplateNameEn)
                .ToArray();

            var trend = scoredResponses
                .GroupBy(x => DateOnly.FromDateTime(x.SubmittedOnUtc))
                .Select(x => new SatisfactionTrendItemResponse
                {
                    Date = x.Key,
                    Score = Round(x.Average(response => response.ScorePercentage)),
                    ResponsesCount = x.Count()
                })
                .OrderBy(x => x.Date)
                .ToArray();

            var complaintsPercentage = totalResponses == 0
                ? 0m
                : Round((decimal)complaintsCount / totalResponses * 100m);

            return new GetBranchSatisfactionReportResponse
            {
                Period = new SatisfactionReportPeriodResponse
                {
                    From = period.From,
                    To = period.To,
                    IsDefaultPeriod = period.IsDefaultPeriod,
                    PeriodSource = period.PeriodSource
                },

                Overall = new SatisfactionOverallResponse
                {
                    Score = overallScore,
                    TotalResponses = totalResponses,
                    ScoredResponses = scoredResponsesCount,
                    UnscoredResponses = unscoredResponses,
                    TotalScoredAnswers = scoredResponses.Sum(x => x.MaxScore / 5),
                    SatisfiedResponses = satisfiedResponses,
                    NeutralResponses = neutralResponses,
                    UnsatisfiedResponses = unsatisfiedResponses,
                    ComplaintsCount = complaintsCount,
                    VoiceAnswersCount = voiceAnswersCount
                },

                Distribution = distribution,
                ByTemplate = byTemplate,
                Trend = trend,

                Complaints = new SatisfactionComplaintsResponse
                {
                    Count = complaintsCount,
                    PercentageOfResponses = complaintsPercentage
                }
            };
        }

        private static IReadOnlyCollection<SatisfactionDistributionItemResponse> BuildDistribution(
            IReadOnlyCollection<int> values)
        {
            var total = values.Count;

            return Enumerable
                .Range(1, 5)
                .Select(value =>
                {
                    var count = values.Count(x => x == value);

                    return new SatisfactionDistributionItemResponse
                    {
                        Value = value,
                        LabelEn = GetDistributionLabelEn(value),
                        LabelAr = GetDistributionLabelAr(value),
                        Count = count,
                        Percentage = total == 0
                            ? 0m
                            : Round((decimal)count / total * 100m)
                    };
                })
                .ToArray();
        }

        private static int ToDistributionValue(decimal scorePercentage)
        {
            if (scorePercentage <= 20m)
            {
                return 1;
            }

            if (scorePercentage <= 40m)
            {
                return 2;
            }

            if (scorePercentage <= 60m)
            {
                return 3;
            }

            if (scorePercentage <= 80m)
            {
                return 4;
            }

            return 5;
        }

        private static decimal Round(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static string GetDistributionLabelEn(int value)
        {
            return value switch
            {
                1 => "Very Unsatisfied",
                2 => "Unsatisfied",
                3 => "Neutral",
                4 => "Satisfied",
                5 => "Very Satisfied",
                _ => string.Empty
            };
        }

        private static string GetDistributionLabelAr(int value)
        {
            return value switch
            {
                1 => "غير راضٍ جدًا",
                2 => "غير راضٍ",
                3 => "محايد",
                4 => "راضٍ",
                5 => "راضٍ جدًا",
                _ => string.Empty
            };
        }

        private sealed record ResolvedSatisfactionPeriod(
            DateOnly From,
            DateOnly To,
            bool IsDefaultPeriod,
            string PeriodSource);
    }
}