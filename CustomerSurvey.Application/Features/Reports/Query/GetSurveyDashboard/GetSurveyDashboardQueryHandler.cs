using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal sealed class GetSurveyDashboardQueryHandler
    : IQueryHandler<GetSurveyDashboardQuery, SurveyDashboardResponse>
{
    private const int DefaultPeriodDays = 30;
    private const int MaxCustomInputsToReturn = 5;
    private const int MaxSegmentsPerCustomInput = 10;
    private const int CriticalCustomInputsPreviewCount = 5;

    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
    private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _surveyCustomInputValueReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyAnswer> _anonymousSurveyAnswerReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> _anonymousCustomInputValueReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetSurveyDashboardQueryHandler(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
        IWriteReadRepository<BranchUser> branchUserReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> surveyCustomInputValueReadRepository,
        IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
        IWriteReadRepository<AnonymousSurveyAnswer> anonymousSurveyAnswerReadRepository,
        IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> anonymousCustomInputValueReadRepository,
        ICurrentUser currentUser)
    {
        _superAdminReadRepository = superAdminReadRepository
            ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
        _branchAdminReadRepository = branchAdminReadRepository
            ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));
        _branchUserReadRepository = branchUserReadRepository
            ?? throw new ArgumentNullException(nameof(branchUserReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _templateReadRepository = templateReadRepository
            ?? throw new ArgumentNullException(nameof(templateReadRepository));
        _anonymousTemplateReadRepository = anonymousTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));
        _surveyResponseReadRepository = surveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));
        _surveyAnswerReadRepository = surveyAnswerReadRepository
            ?? throw new ArgumentNullException(nameof(surveyAnswerReadRepository));
        _surveyCustomInputValueReadRepository = surveyCustomInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(surveyCustomInputValueReadRepository));
        _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseReadRepository));
        _anonymousSurveyAnswerReadRepository = anonymousSurveyAnswerReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousSurveyAnswerReadRepository));
        _anonymousCustomInputValueReadRepository = anonymousCustomInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousCustomInputValueReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<SurveyDashboardResponse>> Handle(
        GetSurveyDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<SurveyDashboardResponse>.Fail(new Error(
                Code: "Reports.SurveyDashboard.Unauthenticated",
                Message: ErrorMessage.GetSurveyDashboard_Unauthenticated,
                Type: ErrorType.Security));
        }

        var actor = await ResolveActorAsync(
            _currentUser.UserId.Value,
            cancellationToken);

        if (actor is null)
        {
            return Result<SurveyDashboardResponse>.Fail(new Error(
                Code: "Reports.SurveyDashboard.CurrentActorNotFound",
                Message: ErrorMessage.GetSurveyDashboard_CurrentActor_NotFound,
                Type: ErrorType.NotFound));
        }

        var sourceValidationError = ValidateSourceFilters(request);
        if (sourceValidationError is not null)
        {
            return Result<SurveyDashboardResponse>.Fail(sourceValidationError);
        }

        var scopeResult = await ResolveScopeAsync(
            request,
            actor,
            cancellationToken);

        if (scopeResult.Error is not null)
        {
            return Result<SurveyDashboardResponse>.Fail(scopeResult.Error);
        }

        var scope = scopeResult.Scope!;
        var periodResult = ResolvePeriod(request);
        if (periodResult.Error is not null)
        {
            return Result<SurveyDashboardResponse>.Fail(periodResult.Error);
        }

        var period = periodResult.Period!;
        var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var includeInternal = request.Source is SurveyDashboardSource.All or SurveyDashboardSource.Internal;
        var includeAnonymous = request.Source is SurveyDashboardSource.All or SurveyDashboardSource.Anonymous;

        var templates = Array.Empty<SurveyDashboardTemplateRow>() as IReadOnlyCollection<SurveyDashboardTemplateRow>;
        var anonymousTemplates = Array.Empty<SurveyDashboardTemplateRow>() as IReadOnlyCollection<SurveyDashboardTemplateRow>;
        var responses = new List<SurveyDashboardResponseRow>();
        var answers = new List<SurveyDashboardAnswerRow>();
        var customInputValues = new List<SurveyDashboardCustomInputValueRow>();

        if (includeInternal)
        {
            templates = await _templateReadRepository.ListAsync(
                new GetSurveyDashboardTemplatesSpec(scope.BranchId),
                cancellationToken);

            if (request.TemplateId.HasValue &&
                !templates.Any(x => x.TemplateId == request.TemplateId.Value))
            {
                return Result<SurveyDashboardResponse>.Fail(new Error(
                    Code: "Reports.SurveyDashboard.TemplateNotFound",
                    Message: ErrorMessage.GetSurveyDashboard_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            responses.AddRange(await _surveyResponseReadRepository.ListAsync(
                new GetSurveyDashboardInternalResponsesSpec(
                    scope.BranchId,
                    fromUtc,
                    toExclusiveUtc,
                    request.TemplateId),
                cancellationToken));

            answers.AddRange(await _surveyAnswerReadRepository.ListAsync(
                new GetSurveyDashboardInternalAnswersSpec(
                    scope.BranchId,
                    fromUtc,
                    toExclusiveUtc,
                    request.TemplateId),
                cancellationToken));

            customInputValues.AddRange(await _surveyCustomInputValueReadRepository.ListAsync(
                new GetSurveyDashboardInternalCustomInputValuesSpec(
                    scope.BranchId,
                    fromUtc,
                    toExclusiveUtc,
                    request.TemplateId),
                cancellationToken));
        }

        if (includeAnonymous)
        {
            anonymousTemplates = await _anonymousTemplateReadRepository.ListAsync(
                new GetSurveyDashboardAnonymousTemplatesSpec(scope.BranchId),
                cancellationToken);

            if (request.AnonymousTemplateId.HasValue &&
                !anonymousTemplates.Any(x => x.TemplateId == request.AnonymousTemplateId.Value))
            {
                return Result<SurveyDashboardResponse>.Fail(new Error(
                    Code: "Reports.SurveyDashboard.AnonymousTemplateNotFound",
                    Message: ErrorMessage.GetSurveyDashboard_AnonymousTemplate_NotFound,
                    Type: ErrorType.NotFound));
            }

            responses.AddRange(await _anonymousSurveyResponseReadRepository.ListAsync(
                new GetSurveyDashboardAnonymousResponsesSpec(
                    scope.BranchId,
                    fromUtc,
                    toExclusiveUtc,
                    request.AnonymousTemplateId),
                cancellationToken));

            answers.AddRange(await _anonymousSurveyAnswerReadRepository.ListAsync(
                new GetSurveyDashboardAnonymousAnswersSpec(
                    scope.BranchId,
                    fromUtc,
                    toExclusiveUtc,
                    request.AnonymousTemplateId),
                cancellationToken));

            customInputValues.AddRange(await _anonymousCustomInputValueReadRepository.ListAsync(
                new GetSurveyDashboardAnonymousCustomInputValuesSpec(
                    scope.BranchId,
                    fromUtc,
                    toExclusiveUtc,
                    request.AnonymousTemplateId),
                cancellationToken));
        }

        var response = BuildResponse(
            request,
            actor,
            scope,
            period,
            templates,
            anonymousTemplates,
            responses,
            answers,
            customInputValues);

        return Result<SurveyDashboardResponse>.Ok(response);
    }

    private async Task<CurrentSurveyDashboardActor?> ResolveActorAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == applicationUserId,
            cancellationToken);

        if (isSuperAdmin)
        {
            return CurrentSurveyDashboardActor.SuperAdmin();
        }

        var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchAdminForSurveyDashboardSpec(applicationUserId),
            cancellationToken);

        if (branchAdmin is not null)
        {
            return CurrentSurveyDashboardActor.BranchAdmin(branchAdmin);
        }

        var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchUserForSurveyDashboardSpec(applicationUserId),
            cancellationToken);

        return branchUser is null
            ? null
            : CurrentSurveyDashboardActor.BranchUser(branchUser);
    }

    private async Task<ScopeResolveResult> ResolveScopeAsync(
        GetSurveyDashboardQuery request,
        CurrentSurveyDashboardActor actor,
        CancellationToken cancellationToken)
    {
        if (!actor.IsSuperAdmin)
        {
            if (request.BranchId.HasValue)
            {
                return ScopeResolveResult.Fail(new Error(
                    Code: "Reports.SurveyDashboard.BranchIdNotAllowed",
                    Message: ErrorMessage.GetSurveyDashboard_BranchId_NotAllowed,
                    Type: ErrorType.Security));
            }

            var branch = new SurveyDashboardBranchRow
            {
                BranchId = actor.BranchId!.Value,
                BranchNameEn = actor.BranchNameEn!,
                BranchNameAr = actor.BranchNameAr,
                IsActive = true
            };

            return ScopeResolveResult.Ok(new ResolvedSurveyDashboardScope(
                BranchId: branch.BranchId,
                BranchNameEn: branch.BranchNameEn,
                BranchNameAr: branch.BranchNameAr,
                DataScope: "CurrentBranch",
                Branches: new[] { branch }));
        }

        var branches = await _branchReadRepository.ListAsync(
            new GetSurveyDashboardBranchesSpec(request.BranchId),
            cancellationToken);

        if (request.BranchId.HasValue && branches.Count == 0)
        {
            return ScopeResolveResult.Fail(new Error(
                Code: "Reports.SurveyDashboard.BranchNotFound",
                Message: ErrorMessage.GetSurveyDashboard_Branch_NotFound,
                Type: ErrorType.NotFound));
        }

        var selectedBranch = request.BranchId.HasValue
            ? branches.First()
            : null;

        return ScopeResolveResult.Ok(new ResolvedSurveyDashboardScope(
            BranchId: selectedBranch?.BranchId,
            BranchNameEn: selectedBranch?.BranchNameEn,
            BranchNameAr: selectedBranch?.BranchNameAr,
            DataScope: request.BranchId.HasValue ? "SpecificBranch" : "AllBranches",
            Branches: branches));
    }

    private static Error? ValidateSourceFilters(GetSurveyDashboardQuery request)
    {
        if (request.Source == SurveyDashboardSource.All)
        {
            if (request.TemplateId.HasValue)
            {
                return new Error(
                    Code: "Reports.SurveyDashboard.TemplateIdNotAllowed",
                    Message: ErrorMessage.GetSurveyDashboard_TemplateId_NotAllowed,
                    Type: ErrorType.Validation);
            }

            if (request.AnonymousTemplateId.HasValue)
            {
                return new Error(
                    Code: "Reports.SurveyDashboard.AnonymousTemplateIdNotAllowed",
                    Message: ErrorMessage.GetSurveyDashboard_AnonymousTemplateId_NotAllowed,
                    Type: ErrorType.Validation);
            }
        }

        if (request.Source == SurveyDashboardSource.Internal &&
            request.AnonymousTemplateId.HasValue)
        {
            return new Error(
                Code: "Reports.SurveyDashboard.AnonymousTemplateIdNotAllowed",
                Message: ErrorMessage.GetSurveyDashboard_AnonymousTemplateId_NotAllowed,
                Type: ErrorType.Validation);
        }

        if (request.Source == SurveyDashboardSource.Anonymous &&
            request.TemplateId.HasValue)
        {
            return new Error(
                Code: "Reports.SurveyDashboard.TemplateIdNotAllowed",
                Message: ErrorMessage.GetSurveyDashboard_TemplateId_NotAllowed,
                Type: ErrorType.Validation);
        }

        return null;
    }

    private static PeriodResolveResult ResolvePeriod(GetSurveyDashboardQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = request.To.HasValue
            ? DateOnly.FromDateTime(request.To.Value)
            : today;

        var from = request.From.HasValue
            ? DateOnly.FromDateTime(request.From.Value)
            : to.AddDays(-DefaultPeriodDays);

        if (from > to)
        {
            return PeriodResolveResult.Fail(new Error(
                Code: "Reports.SurveyDashboard.DateRangeInvalid",
                Message: ErrorMessage.GetSurveyDashboard_DateRange_Invalid,
                Type: ErrorType.Validation));
        }

        return PeriodResolveResult.Ok(new ResolvedSurveyDashboardPeriod(
            From: from,
            To: to,
            IsDefaultPeriod: !request.From.HasValue && !request.To.HasValue));
    }

    private static SurveyDashboardResponse BuildResponse(
        GetSurveyDashboardQuery request,
        CurrentSurveyDashboardActor actor,
        ResolvedSurveyDashboardScope scope,
        ResolvedSurveyDashboardPeriod period,
        IReadOnlyCollection<SurveyDashboardTemplateRow> internalTemplates,
        IReadOnlyCollection<SurveyDashboardTemplateRow> anonymousTemplates,
        IReadOnlyCollection<SurveyDashboardResponseRow> responses,
        IReadOnlyCollection<SurveyDashboardAnswerRow> answers,
        IReadOnlyCollection<SurveyDashboardCustomInputValueRow> customInputValues)
    {
        var scoredResponses = responses
            .Where(x => x.MaxScore > 0)
            .ToArray();

        var internalResponses = responses
            .Where(x => x.Source == SurveyDashboardSource.Internal)
            .ToArray();

        var anonymousResponses = responses
            .Where(x => x.Source == SurveyDashboardSource.Anonymous)
            .ToArray();

        var fromUtc = period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

        return new SurveyDashboardResponse
        {
            Period = new SurveyDashboardPeriodResponse
            {
                From = fromUtc,
                To = toExclusiveUtc.AddTicks(-1),
                IsDefaultPeriod = period.IsDefaultPeriod,
                GroupBy = request.GroupBy
            },

            Scope = new SurveyDashboardScopeResponse
            {
                ActorScope = actor.ActorScope,
                DataScope = scope.DataScope,
                BranchId = scope.BranchId,
                BranchNameEn = scope.BranchNameEn,
                BranchNameAr = scope.BranchNameAr
            },

            Filters = new SurveyDashboardFiltersResponse
            {
                Source = request.Source,
                BranchId = request.BranchId,
                TemplateId = request.TemplateId,
                AnonymousTemplateId = request.AnonymousTemplateId,
                TopQuestionsCount = request.TopQuestionsCount,
                CriticalResponsesCount = request.CriticalResponsesCount,
                CriticalScoreThreshold = request.CriticalScoreThreshold
            },

            Summary = new SurveyDashboardSummaryResponse
            {
                TotalResponses = responses.Count,
                ScoredResponses = scoredResponses.Length,
                UnscoredResponses = responses.Count - scoredResponses.Length,
                InternalResponses = internalResponses.Length,
                AnonymousResponses = anonymousResponses.Length,
                InternalScoredResponses = internalResponses.Count(IsScored),
                AnonymousScoredResponses = anonymousResponses.Count(IsScored),
                AverageScorePercentage = AverageScore(scoredResponses),
                SatisfiedResponses = scoredResponses.Count(IsSatisfied),
                NeutralResponses = scoredResponses.Count(IsNeutral),
                UnhappyResponses = scoredResponses.Count(IsUnhappy),
                ComplaintsCount = CountComplaints(answers),
                VoiceAnswersCount = CountVoiceAnswers(answers),
                ActiveInternalTemplatesCount = internalTemplates.Count(x => x.IsActive),
                ActiveAnonymousTemplatesCount = anonymousTemplates.Count(x => x.IsActive),
                TemplatesWithResponsesCount = responses
                    .Select(x => $"{x.Source}:{x.TemplateId}")
                    .Distinct()
                    .Count(),
                BranchesCount = scope.Branches.Count,
                BranchesWithResponsesCount = responses
                    .Select(x => x.BranchId)
                    .Distinct()
                    .Count()
            },

            SourceBreakdown = new SurveyDashboardSourceBreakdownResponse
            {
                Internal = BuildSourceSummary(
                    internalResponses,
                    answers.Where(x => x.Source == SurveyDashboardSource.Internal)),

                Anonymous = BuildSourceSummary(
                    anonymousResponses,
                    answers.Where(x => x.Source == SurveyDashboardSource.Anonymous))
            },

            BranchesSummary = BuildBranchesSummary(
                scope,
                responses,
                answers,
                actor.IsSuperAdmin),

            SatisfactionTrend = BuildTrend(
                responses,
                request.GroupBy),

            TemplatePerformance = BuildTemplatePerformance(
                responses,
                answers),

            LowestRatedQuestions = BuildLowestRatedQuestions(
                answers,
                request.TopQuestionsCount),

            CustomInputSegments = BuildCustomInputSegments(
                request.Source,
                customInputValues),

            CriticalResponses = BuildCriticalResponses(
                responses,
                answers,
                customInputValues,
                request.CriticalScoreThreshold,
                request.CriticalResponsesCount)
        };
    }

    private static SurveyDashboardSourceSummaryResponse BuildSourceSummary(
        IReadOnlyCollection<SurveyDashboardResponseRow> responses,
        IEnumerable<SurveyDashboardAnswerRow> answers)
    {
        var answersArray = answers.ToArray();
        var scored = responses.Where(IsScored).ToArray();

        return new SurveyDashboardSourceSummaryResponse
        {
            ResponsesCount = responses.Count,
            ScoredResponsesCount = scored.Length,
            AverageScorePercentage = AverageScore(scored),
            SatisfiedResponses = scored.Count(IsSatisfied),
            NeutralResponses = scored.Count(IsNeutral),
            UnhappyResponses = scored.Count(IsUnhappy),
            ComplaintsCount = CountComplaints(answersArray),
            VoiceAnswersCount = CountVoiceAnswers(answersArray)
        };
    }

    private static IReadOnlyCollection<SurveyDashboardBranchSummaryResponse> BuildBranchesSummary(
        ResolvedSurveyDashboardScope scope,
        IReadOnlyCollection<SurveyDashboardResponseRow> responses,
        IReadOnlyCollection<SurveyDashboardAnswerRow> answers,
        bool isSuperAdmin)
    {
        var responsesByBranchId = responses
            .GroupBy(x => x.BranchId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var complaintsByBranchId = answers
            .Where(IsComplaint)
            .GroupBy(x => x.BranchId)
            .ToDictionary(x => x.Key, x => x.Count());

        var voiceAnswersByBranchId = answers
            .Where(IsVoiceAnswer)
            .GroupBy(x => x.BranchId)
            .ToDictionary(x => x.Key, x => x.Count());

        return scope.Branches
            .Select(branch =>
            {
                responsesByBranchId.TryGetValue(branch.BranchId, out var branchResponses);
                branchResponses ??= Array.Empty<SurveyDashboardResponseRow>();

                var scored = branchResponses.Where(IsScored).ToArray();

                complaintsByBranchId.TryGetValue(branch.BranchId, out var complaintsCount);
                voiceAnswersByBranchId.TryGetValue(branch.BranchId, out var voiceAnswersCount);

                return new SurveyDashboardBranchSummaryResponse
                {
                    BranchId = branch.BranchId,
                    BranchNameEn = branch.BranchNameEn,
                    BranchNameAr = branch.BranchNameAr,
                    TotalResponses = branchResponses.Length,
                    InternalResponses = branchResponses.Count(x => x.Source == SurveyDashboardSource.Internal),
                    AnonymousResponses = branchResponses.Count(x => x.Source == SurveyDashboardSource.Anonymous),
                    AverageScorePercentage = AverageScore(scored),
                    ComplaintsCount = complaintsCount,
                    VoiceAnswersCount = voiceAnswersCount,
                    DetailsNavigation = BuildBranchDashboardNavigation(branch.BranchId, isSuperAdmin)
                };
            })
            .OrderByDescending(x => x.TotalResponses)
            .ThenBy(x => x.BranchNameEn)
            .ToArray();
    }

    private static IReadOnlyCollection<SurveyDashboardTrendItemResponse> BuildTrend(
        IReadOnlyCollection<SurveyDashboardResponseRow> responses,
        DashboardGroupBy groupBy)
    {
        return responses
            .GroupBy(x => GetPeriodKey(x.SubmittedOnUtc, groupBy))
            .OrderBy(x => x.Key)
            .Select(x =>
            {
                var scored = x.Where(IsScored).ToArray();
                var internalScored = x
                    .Where(r => r.Source == SurveyDashboardSource.Internal && IsScored(r))
                    .ToArray();
                var anonymousScored = x
                    .Where(r => r.Source == SurveyDashboardSource.Anonymous && IsScored(r))
                    .ToArray();

                return new SurveyDashboardTrendItemResponse
                {
                    Period = x.Key,
                    ResponsesCount = x.Count(),
                    InternalResponses = x.Count(r => r.Source == SurveyDashboardSource.Internal),
                    AnonymousResponses = x.Count(r => r.Source == SurveyDashboardSource.Anonymous),
                    AverageScorePercentage = AverageScore(scored),
                    InternalAverageScorePercentage = internalScored.Length == 0 ? null : AverageScore(internalScored),
                    AnonymousAverageScorePercentage = anonymousScored.Length == 0 ? null : AverageScore(anonymousScored)
                };
            })
            .ToArray();
    }

    private static IReadOnlyCollection<SurveyDashboardTemplatePerformanceItemResponse> BuildTemplatePerformance(
        IReadOnlyCollection<SurveyDashboardResponseRow> responses,
        IReadOnlyCollection<SurveyDashboardAnswerRow> answers)
    {
        var complaintsByTemplate = answers
            .Where(IsComplaint)
            .GroupBy(x => new { x.Source, x.TemplateId })
            .ToDictionary(x => (x.Key.Source, x.Key.TemplateId), x => x.Count());

        var voicesByTemplate = answers
            .Where(IsVoiceAnswer)
            .GroupBy(x => new { x.Source, x.TemplateId })
            .ToDictionary(x => (x.Key.Source, x.Key.TemplateId), x => x.Count());

        return responses
            .GroupBy(x => new
            {
                x.Source,
                x.TemplateId,
                x.TemplateNameEn,
                x.TemplateNameAr,
                x.BranchId,
                x.BranchNameEn,
                x.BranchNameAr
            })
            .Select(x =>
            {
                var scored = x.Where(IsScored).ToArray();
                var key = (x.Key.Source, x.Key.TemplateId);

                complaintsByTemplate.TryGetValue(key, out var complaintsCount);
                voicesByTemplate.TryGetValue(key, out var voiceAnswersCount);

                var average = AverageScore(scored);

                return new SurveyDashboardTemplatePerformanceItemResponse
                {
                    Source = x.Key.Source,
                    TemplateId = x.Key.TemplateId,
                    TemplateNameEn = x.Key.TemplateNameEn,
                    TemplateNameAr = x.Key.TemplateNameAr,
                    BranchId = x.Key.BranchId,
                    BranchNameEn = x.Key.BranchNameEn,
                    BranchNameAr = x.Key.BranchNameAr,
                    ResponsesCount = x.Count(),
                    ScoredResponsesCount = scored.Length,
                    AverageScorePercentage = average,
                    ComplaintsCount = complaintsCount,
                    VoiceAnswersCount = voiceAnswersCount,
                    RiskLevel = ResolveRiskLevel(average),
                    DetailsNavigation = BuildTemplateResponsesNavigation(x.Key.Source, x.Key.TemplateId)
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.ResponsesCount)
            .ToArray();
    }

    private static IReadOnlyCollection<SurveyDashboardLowestRatedQuestionItemResponse> BuildLowestRatedQuestions(
        IReadOnlyCollection<SurveyDashboardAnswerRow> answers,
        int topQuestionsCount)
    {
        return answers
            .Select(x => new
            {
                Answer = x,
                Value = GetQuestionScoreValue(x)
            })
            .Where(x => x.Value.HasValue)
            .GroupBy(x => new
            {
                x.Answer.Source,
                x.Answer.TemplateId,
                x.Answer.TemplateNameEn,
                x.Answer.TemplateNameAr,
                x.Answer.BranchId,
                x.Answer.BranchNameEn,
                x.Answer.BranchNameAr,
                x.Answer.QuestionId,
                x.Answer.QuestionTextEn,
                x.Answer.QuestionTextAr,
                x.Answer.QuestionType
            })
            .Select(x =>
            {
                var averageValue = Round(x.Average(a => (decimal)a.Value!.Value));

                return new SurveyDashboardLowestRatedQuestionItemResponse
                {
                    Source = x.Key.Source,
                    TemplateId = x.Key.TemplateId,
                    TemplateNameEn = x.Key.TemplateNameEn,
                    TemplateNameAr = x.Key.TemplateNameAr,
                    BranchId = x.Key.BranchId,
                    BranchNameEn = x.Key.BranchNameEn,
                    BranchNameAr = x.Key.BranchNameAr,
                    QuestionId = x.Key.QuestionId,
                    QuestionTextEn = x.Key.QuestionTextEn,
                    QuestionTextAr = x.Key.QuestionTextAr,
                    QuestionType = x.Key.QuestionType,
                    QuestionTypeName = x.Key.QuestionType.ToString(),
                    AnswersCount = x.Count(),
                    AverageValue = averageValue,
                    AverageScorePercentage = Round(averageValue / 5m * 100m),
                    DetailsNavigation = BuildQuestionContextNavigation(x.Key.Source, x.Key.TemplateId)
                };
            })
            .OrderBy(x => x.AverageScorePercentage)
            .ThenByDescending(x => x.AnswersCount)
            .Take(topQuestionsCount)
            .ToArray();
    }

    private static IReadOnlyCollection<SurveyDashboardCustomInputSegmentResponse> BuildCustomInputSegments(
        SurveyDashboardSource requestedSource,
        IReadOnlyCollection<SurveyDashboardCustomInputValueRow> customInputValues)
    {
        return customInputValues
            .Where(x => x.MaxScore > 0)
            .Select(x => new
            {
                Value = x,
                DisplayValue = GetCustomInputValueText(x)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.DisplayValue))
            .GroupBy(x => new
            {
                x.Value.NameSnapshot,
                x.Value.LabelEn,
                x.Value.LabelAr,
                x.Value.TypeSnapshot
            })
            .OrderByDescending(x => x.Count())
            .Take(MaxCustomInputsToReturn)
            .Select(inputGroup => new SurveyDashboardCustomInputSegmentResponse
            {
                Source = requestedSource == SurveyDashboardSource.All
                    ? SurveyDashboardSource.All
                    : inputGroup.First().Value.Source,
                CustomInputName = inputGroup.Key.NameSnapshot,
                LabelEn = inputGroup.Key.LabelEn,
                LabelAr = inputGroup.Key.LabelAr,
                Type = inputGroup.Key.TypeSnapshot,
                TypeName = inputGroup.Key.TypeSnapshot.ToString(),
                Segments = inputGroup
                    .GroupBy(x => x.DisplayValue)
                    .OrderByDescending(x => x.Count())
                    .Take(MaxSegmentsPerCustomInput)
                    .Select(valueGroup => new SurveyDashboardCustomInputSegmentItemResponse
                    {
                        Value = valueGroup.Key,
                        ResponsesCount = valueGroup.Count(),
                        InternalResponses = valueGroup.Count(x => x.Value.Source == SurveyDashboardSource.Internal),
                        AnonymousResponses = valueGroup.Count(x => x.Value.Source == SurveyDashboardSource.Anonymous),
                        AverageScorePercentage = Round(valueGroup.Average(x => x.Value.ScorePercentage)),
                        DetailsNavigation = null
                    })
                    .ToArray()
            })
            .Where(x => x.Segments.Count > 0)
            .ToArray();
    }

    private static IReadOnlyCollection<SurveyDashboardCriticalResponseItemResponse> BuildCriticalResponses(
        IReadOnlyCollection<SurveyDashboardResponseRow> responses,
        IReadOnlyCollection<SurveyDashboardAnswerRow> answers,
        IReadOnlyCollection<SurveyDashboardCustomInputValueRow> customInputValues,
        decimal criticalScoreThreshold,
        int criticalResponsesCount)
    {
        var complaintsByResponseId = answers
            .Where(IsComplaint)
            .GroupBy(x => (x.Source, x.ResponseId))
            .ToDictionary(x => x.Key, x => x.First().TextAnswer);

        var customInputsByResponseId = customInputValues
            .GroupBy(x => (x.Source, x.ResponseId))
            .ToDictionary(
                x => x.Key,
                x => x
                    .Take(CriticalCustomInputsPreviewCount)
                    .Select(MapCustomInputPreview)
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
                var key = (x.Source, x.ResponseId);

                complaintsByResponseId.TryGetValue(key, out var complaintText);
                customInputsByResponseId.TryGetValue(key, out var customInputs);

                return new SurveyDashboardCriticalResponseItemResponse
                {
                    Source = x.Source,
                    ResponseId = x.ResponseId,
                    TemplateId = x.TemplateId,
                    TemplateNameEn = x.TemplateNameEn,
                    TemplateNameAr = x.TemplateNameAr,
                    BranchId = x.BranchId,
                    BranchNameEn = x.BranchNameEn,
                    BranchNameAr = x.BranchNameAr,
                    SubmittedOnUtc = x.SubmittedOnUtc,
                    ScorePercentage = x.ScorePercentage,
                    ComplaintText = complaintText,
                    HasComplaint = x.HasComplaint,
                    HasVoice = x.HasVoice,
                    OperatorId = x.OperatorId,
                    OperatorNameEn = x.OperatorNameEn,
                    OperatorNameAr = x.OperatorNameAr,
                    CustomInputsPreview = customInputs ?? Array.Empty<SurveyDashboardCustomInputPreviewResponse>(),
                    DetailsNavigation = BuildResponseDetailsNavigation(x.Source, x.TemplateId, x.ResponseId)
                };
            })
            .ToArray();
    }

    private static int? GetQuestionScoreValue(SurveyDashboardAnswerRow answer)
    {
        return answer.QuestionType switch
        {
            QuestionType.StarRating => answer.StarRatingValue,
            QuestionType.Smiles => answer.SmileValue,
            QuestionType.SingleChoice => answer.SelectedQuestionOptionValue,
            _ => null
        };
    }

    private static SurveyDashboardCustomInputPreviewResponse MapCustomInputPreview(
        SurveyDashboardCustomInputValueRow value)
    {
        return new SurveyDashboardCustomInputPreviewResponse
        {
            Name = value.NameSnapshot,
            LabelEn = value.LabelEn,
            LabelAr = value.LabelAr,
            Value = GetCustomInputValueText(value)
        };
    }

    private static string GetCustomInputValueText(SurveyDashboardCustomInputValueRow value)
    {
        return value.TypeSnapshot switch
        {
            TemplateCustomInputType.String => value.StringValue?.Trim() ?? string.Empty,
            TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };
    }

    private static string GetPeriodKey(DateTime submittedOnUtc, DashboardGroupBy groupBy)
    {
        return groupBy == DashboardGroupBy.Month
            ? $"{submittedOnUtc.Year:D4}-{submittedOnUtc.Month:D2}"
            : DateOnly.FromDateTime(submittedOnUtc).ToString("yyyy-MM-dd");
    }

    private static bool IsScored(SurveyDashboardResponseRow response)
    {
        return response.MaxScore > 0;
    }

    private static bool IsSatisfied(SurveyDashboardResponseRow response)
    {
        return response.ScorePercentage >= 80m;
    }

    private static bool IsNeutral(SurveyDashboardResponseRow response)
    {
        return response.ScorePercentage >= 60m && response.ScorePercentage < 80m;
    }

    private static bool IsUnhappy(SurveyDashboardResponseRow response)
    {
        return response.ScorePercentage < 60m;
    }

    private static bool IsComplaint(SurveyDashboardAnswerRow answer)
    {
        return answer.QuestionType == QuestionType.Complain &&
               !string.IsNullOrWhiteSpace(answer.TextAnswer);
    }

    private static bool IsVoiceAnswer(SurveyDashboardAnswerRow answer)
    {
        return answer.QuestionType == QuestionType.Voice &&
               !string.IsNullOrWhiteSpace(answer.VoiceFileName);
    }

    private static int CountComplaints(IEnumerable<SurveyDashboardAnswerRow> answers)
    {
        return answers.Count(IsComplaint);
    }

    private static int CountVoiceAnswers(IEnumerable<SurveyDashboardAnswerRow> answers)
    {
        return answers.Count(IsVoiceAnswer);
    }

    private static decimal AverageScore(IReadOnlyCollection<SurveyDashboardResponseRow> scoredResponses)
    {
        return scoredResponses.Count == 0
            ? 0m
            : Round(scoredResponses.Average(x => x.ScorePercentage));
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

    private static SurveyDashboardDetailsNavigationResponse BuildBranchDashboardNavigation(
        Guid branchId,
        bool isSuperAdmin)
    {
        var path = isSuperAdmin
            ? $"/api/reports/survey-dashboard?branchId={branchId}&source=All"
            : "/api/reports/survey-dashboard?source=All";

        return new SurveyDashboardDetailsNavigationResponse
        {
            RouteType = "BranchDashboard",
            Method = "GET",
            Path = path
        };
    }

    private static SurveyDashboardDetailsNavigationResponse BuildTemplateResponsesNavigation(
        SurveyDashboardSource source,
        Guid templateId)
    {
        return source == SurveyDashboardSource.Anonymous
            ? new SurveyDashboardDetailsNavigationResponse
            {
                RouteType = "AnonymousTemplateResponses",
                Method = "GET",
                Path = $"/api/anonymous-templates/{templateId}/responses?pageNumber=1&pageSize=10"
            }
            : new SurveyDashboardDetailsNavigationResponse
            {
                RouteType = "InternalTemplateResponses",
                Method = "GET",
                Path = $"/api/reports/branch-responses?templateId={templateId}&pageNumber=1&pageSize=10"
            };
    }

    private static SurveyDashboardDetailsNavigationResponse BuildQuestionContextNavigation(
        SurveyDashboardSource source,
        Guid templateId)
    {
        return source == SurveyDashboardSource.Anonymous
            ? new SurveyDashboardDetailsNavigationResponse
            {
                RouteType = "AnonymousTemplateResponsesByQuestionContext",
                Method = "GET",
                Path = $"/api/anonymous-templates/{templateId}/responses?pageNumber=1&pageSize=10"
            }
            : new SurveyDashboardDetailsNavigationResponse
            {
                RouteType = "InternalTemplateResponsesByQuestionContext",
                Method = "GET",
                Path = $"/api/reports/branch-responses?templateId={templateId}&pageNumber=1&pageSize=10"
            };
    }

    private static SurveyDashboardDetailsNavigationResponse BuildResponseDetailsNavigation(
        SurveyDashboardSource source,
        Guid templateId,
        Guid responseId)
    {
        return source == SurveyDashboardSource.Anonymous
            ? new SurveyDashboardDetailsNavigationResponse
            {
                RouteType = "AnonymousResponseDetails",
                Method = "GET",
                Path = $"/api/anonymous-templates/{templateId}/responses/{responseId}"
            }
            : new SurveyDashboardDetailsNavigationResponse
            {
                RouteType = "InternalResponseDetails",
                Method = "GET",
                Path = $"/api/reports/branch-responses/{responseId}"
            };
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private sealed record CurrentSurveyDashboardActor(
        bool IsSuperAdmin,
        string ActorScope,
        Guid? BranchId,
        string? BranchNameEn,
        string? BranchNameAr)
    {
        public static CurrentSurveyDashboardActor SuperAdmin()
        {
            return new CurrentSurveyDashboardActor(true, "SuperAdmin", null, null, null);
        }

        public static CurrentSurveyDashboardActor BranchAdmin(CurrentBranchActorForSurveyDashboardDto actor)
        {
            return new CurrentSurveyDashboardActor(
                false,
                "BranchAdmin",
                actor.BranchId,
                actor.BranchNameEn,
                actor.BranchNameAr);
        }

        public static CurrentSurveyDashboardActor BranchUser(CurrentBranchActorForSurveyDashboardDto actor)
        {
            return new CurrentSurveyDashboardActor(
                false,
                "BranchUser",
                actor.BranchId,
                actor.BranchNameEn,
                actor.BranchNameAr);
        }
    }

    private sealed record CurrentBranchActorForSurveyDashboardDto
    {
        public Guid BranchId { get; init; }

        public string BranchNameEn { get; init; } = string.Empty;

        public string? BranchNameAr { get; init; }
    }

    private sealed record ResolvedSurveyDashboardScope(
        Guid? BranchId,
        string? BranchNameEn,
        string? BranchNameAr,
        string DataScope,
        IReadOnlyCollection<SurveyDashboardBranchRow> Branches);

    private sealed record ResolvedSurveyDashboardPeriod(
        DateOnly From,
        DateOnly To,
        bool IsDefaultPeriod);

    private sealed record PeriodResolveResult(
        ResolvedSurveyDashboardPeriod? Period,
        Error? Error)
    {
        public static PeriodResolveResult Ok(ResolvedSurveyDashboardPeriod period)
        {
            return new PeriodResolveResult(period, null);
        }

        public static PeriodResolveResult Fail(Error error)
        {
            return new PeriodResolveResult(null, error);
        }
    }

    private sealed record ScopeResolveResult(
        ResolvedSurveyDashboardScope? Scope,
        Error? Error)
    {
        public static ScopeResolveResult Ok(ResolvedSurveyDashboardScope scope)
        {
            return new ScopeResolveResult(scope, null);
        }

        public static ScopeResolveResult Fail(Error error)
        {
            return new ScopeResolveResult(null, error);
        }
    }

    private sealed class GetCurrentBranchAdminForSurveyDashboardSpec
        : Specification<BranchAdmin, CurrentBranchActorForSurveyDashboardDto>
    {
        public GetCurrentBranchAdminForSurveyDashboardSpec(Guid applicationUserId)
        {
            UseNoTracking();

            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForSurveyDashboardDto
            {
                BranchId = x.BranchId,
                BranchNameEn = x.Branch.NameEn,
                BranchNameAr = x.Branch.NameAr
            });
        }
    }

    private sealed class GetCurrentBranchUserForSurveyDashboardSpec
        : Specification<BranchUser, CurrentBranchActorForSurveyDashboardDto>
    {
        public GetCurrentBranchUserForSurveyDashboardSpec(Guid applicationUserId)
        {
            UseNoTracking();

            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForSurveyDashboardDto
            {
                BranchId = x.BranchId,
                BranchNameEn = x.Branch.NameEn,
                BranchNameAr = x.Branch.NameAr
            });
        }
    }
}
