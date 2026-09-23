using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyResponsesPagination;

internal sealed class GetSurveyResponsesPaginationQueryHandler
    : IQueryHandler<GetSurveyResponsesPaginationQuery, Pagination<SurveyResponsePaginationItemResponse>>
{
    private const int DefaultPeriodDays = 30;
    private const int CustomInputsPreviewCount = 3;

    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _surveyCustomInputValueReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyAnswer> _anonymousSurveyAnswerReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> _anonymousCustomInputValueReadRepository;
    private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionReadRepository;
    private readonly IWriteReadRepository<TemplateQuestionCondition> _templateQuestionConditionReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _anonymousTemplateQuestionConditionReadRepository;
    private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
    private readonly ISurveyReportScoringService _scoringService;
    private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
    private readonly ICurrentUser _currentUser;

    public GetSurveyResponsesPaginationQueryHandler(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> surveyCustomInputValueReadRepository,
        IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
        IWriteReadRepository<AnonymousSurveyAnswer> anonymousSurveyAnswerReadRepository,
        IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> anonymousCustomInputValueReadRepository,
        IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
        IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionReadRepository,
        IWriteReadRepository<TemplateQuestionCondition> templateQuestionConditionReadRepository,
        IWriteReadRepository<AnonymousTemplateQuestionCondition> anonymousTemplateQuestionConditionReadRepository,
        IWriteReadRepository<QuestionOption> questionOptionReadRepository,
        ISurveyReportScoringService scoringService,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        ICurrentUser currentUser)
    {
        _superAdminReadRepository = superAdminReadRepository;
        _branchReadRepository = branchReadRepository;
        _templateReadRepository = templateReadRepository;
        _anonymousTemplateReadRepository = anonymousTemplateReadRepository;
        _surveyResponseReadRepository = surveyResponseReadRepository;
        _surveyAnswerReadRepository = surveyAnswerReadRepository;
        _surveyCustomInputValueReadRepository = surveyCustomInputValueReadRepository;
        _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository;
        _anonymousSurveyAnswerReadRepository = anonymousSurveyAnswerReadRepository;
        _anonymousCustomInputValueReadRepository = anonymousCustomInputValueReadRepository;
        _templateQuestionReadRepository = templateQuestionReadRepository;
        _anonymousTemplateQuestionReadRepository = anonymousTemplateQuestionReadRepository;
        _templateQuestionConditionReadRepository = templateQuestionConditionReadRepository;
        _anonymousTemplateQuestionConditionReadRepository = anonymousTemplateQuestionConditionReadRepository;
        _questionOptionReadRepository = questionOptionReadRepository;
        _scoringService = scoringService;
        _currentBranchScopeResolver = currentBranchScopeResolver;
        _currentUser = currentUser;
    }

    public async Task<Result<Pagination<SurveyResponsePaginationItemResponse>>> Handle(
        GetSurveyResponsesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("Reports.SurveyResponses.Unauthenticated", ErrorMessage.GetSurveyDashboard_Unauthenticated, ErrorType.Security);
        }

        var scopeResult = await ResolveScopeAsync(request, _currentUser.UserId.Value, cancellationToken);
        if (scopeResult.Error is not null)
        {
            return Result<Pagination<SurveyResponsePaginationItemResponse>>.Fail(scopeResult.Error);
        }

        var templateResult = await ResolveTemplateAsync(request, scopeResult.BranchId, cancellationToken);
        if (templateResult.Error is not null)
        {
            return Result<Pagination<SurveyResponsePaginationItemResponse>>.Fail(templateResult.Error);
        }

        var period = ResolvePeriod(request);
        if (period.Error is not null)
        {
            return Result<Pagination<SurveyResponsePaginationItemResponse>>.Fail(period.Error);
        }

        var appliedSource = templateResult.Template?.DashboardSource ?? request.Source;
        var internalTemplateId = templateResult.Template?.TemplateKind == SurveyDashboardTemplateKind.Authorized
            ? templateResult.Template.TemplateId
            : (Guid?)null;
        var anonymousTemplateId = templateResult.Template?.TemplateKind == SurveyDashboardTemplateKind.Anonymous
            ? templateResult.Template.TemplateId
            : (Guid?)null;

        var fromUtc = period.From!.Value.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = period.To!.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var responses = new List<SurveyDashboardResponseRow>();
        var answers = new List<SurveyDashboardAnswerRow>();
        var customInputs = new List<SurveyDashboardCustomInputValueRow>();
        var templateQuestions = new List<TemplateQuestionFlatDto>();
        var conditions = new List<ConditionFlatDto>();

        if (appliedSource is SurveyDashboardSource.All or SurveyDashboardSource.Internal)
        {
            var templates = await _templateReadRepository.ListAsync(
                new GetSurveyDashboardTemplatesSpec(scopeResult.BranchId, internalTemplateId),
                cancellationToken);
            responses.AddRange(await _surveyResponseReadRepository.ListAsync(
                new GetSurveyDashboardInternalResponsesSpec(scopeResult.BranchId, fromUtc, toExclusiveUtc, internalTemplateId),
                cancellationToken));
            answers.AddRange(await _surveyAnswerReadRepository.ListAsync(
                new GetSurveyDashboardInternalAnswersSpec(scopeResult.BranchId, fromUtc, toExclusiveUtc, internalTemplateId),
                cancellationToken));
            customInputs.AddRange(await _surveyCustomInputValueReadRepository.ListAsync(
                new GetSurveyDashboardInternalCustomInputValuesSpec(scopeResult.BranchId, fromUtc, toExclusiveUtc, internalTemplateId),
                cancellationToken));

            var templateIds = templates.Select(x => x.TemplateId).ToArray();
            if (templateIds.Length > 0)
            {
                templateQuestions.AddRange(await _templateQuestionReadRepository.ListAsync(
                    new GetSurveyDashboardTemplateQuestionsSpec(templateIds), cancellationToken));
                conditions.AddRange(await _templateQuestionConditionReadRepository.ListAsync(
                    new GetSurveyDashboardTemplateQuestionConditionsSpec(templateIds), cancellationToken));
            }
        }

        if (appliedSource is SurveyDashboardSource.All or SurveyDashboardSource.Anonymous)
        {
            var templates = await _anonymousTemplateReadRepository.ListAsync(
                new GetSurveyDashboardAnonymousTemplatesSpec(scopeResult.BranchId, anonymousTemplateId),
                cancellationToken);
            responses.AddRange(await _anonymousSurveyResponseReadRepository.ListAsync(
                new GetSurveyDashboardAnonymousResponsesSpec(scopeResult.BranchId, fromUtc, toExclusiveUtc, anonymousTemplateId),
                cancellationToken));
            answers.AddRange(await _anonymousSurveyAnswerReadRepository.ListAsync(
                new GetSurveyDashboardAnonymousAnswersSpec(scopeResult.BranchId, fromUtc, toExclusiveUtc, anonymousTemplateId),
                cancellationToken));
            customInputs.AddRange(await _anonymousCustomInputValueReadRepository.ListAsync(
                new GetSurveyDashboardAnonymousCustomInputValuesSpec(scopeResult.BranchId, fromUtc, toExclusiveUtc, anonymousTemplateId),
                cancellationToken));

            var templateIds = templates.Select(x => x.TemplateId).ToArray();
            if (templateIds.Length > 0)
            {
                templateQuestions.AddRange(await _anonymousTemplateQuestionReadRepository.ListAsync(
                    new GetSurveyDashboardAnonymousTemplateQuestionsSpec(templateIds), cancellationToken));
                conditions.AddRange(await _anonymousTemplateQuestionConditionReadRepository.ListAsync(
                    new GetSurveyDashboardAnonymousTemplateQuestionConditionsSpec(templateIds), cancellationToken));
            }
        }

        var questionIds = templateQuestions.Select(x => x.QuestionId).Distinct().ToArray();
        IReadOnlyCollection<QuestionOptionFlatDto> options = questionIds.Length == 0
            ? Array.Empty<QuestionOptionFlatDto>()
            : await _questionOptionReadRepository.ListAsync(new GetSurveyDashboardQuestionOptionsSpec(questionIds), cancellationToken);

        responses = ApplyCalculatedScores(
            responses,
            answers,
            templateQuestions,
            conditions,
            options,
            request.ScoreCalculationMode);

        var filtered = FilterResponses(request, responses, answers, customInputs);
        var totalCount = filtered.Count;
        var ordered = request.OrderSort == OrderSort.Oldest
            ? filtered.OrderBy(x => x.SubmittedOnUtc).ThenBy(x => x.ResponseId)
            : filtered.OrderByDescending(x => x.SubmittedOnUtc).ThenByDescending(x => x.ResponseId);

        var pageRows = ordered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArray();

        var customInputsByResponse = customInputs
            .GroupBy(x => (x.Source, x.ResponseId))
            .ToDictionary(x => x.Key, x => x.Take(CustomInputsPreviewCount).Select(MapPreview).ToArray());

        var items = pageRows.Select(row =>
        {
            customInputsByResponse.TryGetValue((row.Source, row.ResponseId), out var previews);
            return MapItem(row, previews ?? Array.Empty<SurveyResponseCustomInputPreviewResponse>());
        }).ToArray();

        return Result<Pagination<SurveyResponsePaginationItemResponse>>.Ok(
            new Pagination<SurveyResponsePaginationItemResponse>(request.PageNumber, request.PageSize, totalCount, items));
    }

    private async Task<ScopeResult> ResolveScopeAsync(
        GetSurveyResponsesPaginationQuery request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var isSuperAdmin = await _superAdminReadRepository.AnyAsync(x => x.ApplicationUserId == userId, cancellationToken);
        if (!isSuperAdmin)
        {
            if (request.BranchId.HasValue)
            {
                return ScopeResult.Fail(new Error(
                    "Reports.SurveyResponses.BranchIdNotAllowed",
                    ErrorMessage.GetSurveyDashboard_BranchId_NotAllowed,
                    ErrorType.Security));
            }

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(cancellationToken);
            if (currentBranchScope.IsFailure)
            {
                return ScopeResult.Fail(currentBranchScope.Errors.First());
            }

            var branches = await _branchReadRepository.ListAsync(
                new GetSurveyDashboardBranchesSpec(currentBranchScope.Value.BranchId), cancellationToken);
            if (branches.Count == 0)
            {
                return ScopeResult.Fail(new Error(
                    "Reports.SurveyResponses.CurrentActorNotFound",
                    ErrorMessage.GetSurveyDashboard_CurrentActor_NotFound,
                    ErrorType.NotFound));
            }

            return ScopeResult.Ok(currentBranchScope.Value.BranchId);
        }

        if (request.BranchId.HasValue)
        {
            var branches = await _branchReadRepository.ListAsync(
                new GetSurveyDashboardBranchesSpec(request.BranchId), cancellationToken);
            if (branches.Count == 0)
            {
                return ScopeResult.Fail(new Error(
                    "Reports.SurveyResponses.BranchNotFound",
                    ErrorMessage.GetSurveyDashboard_Branch_NotFound,
                    ErrorType.NotFound));
            }
        }

        return ScopeResult.Ok(request.BranchId);
    }

    private async Task<TemplateResult> ResolveTemplateAsync(
        GetSurveyResponsesPaginationQuery request,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        if (request.TemplateId.HasValue && request.AnonymousTemplateId.HasValue)
        {
            return TemplateResult.Fail(new Error(
                "Reports.SurveyResponses.TemplateFilterAmbiguous",
                ErrorMessage.SurveyDashboard_TemplateFilter_Ambiguous,
                ErrorType.Validation));
        }

        ResolvedSurveyDashboardTemplateFilter? template = null;
        if (request.TemplateId.HasValue)
        {
            template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetAuthorizedTemplateForSurveyDashboardFilterSpec(request.TemplateId.Value, branchId), cancellationToken)
                ?? await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                    new GetAnonymousTemplateForSurveyDashboardFilterSpec(request.TemplateId.Value, branchId), cancellationToken);
        }
        else if (request.AnonymousTemplateId.HasValue)
        {
            template = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForSurveyDashboardFilterSpec(request.AnonymousTemplateId.Value, branchId), cancellationToken);
        }

        if ((request.TemplateId.HasValue || request.AnonymousTemplateId.HasValue) && template is null)
        {
            return TemplateResult.Fail(new Error(
                "Reports.SurveyResponses.TemplateFilterNotFound",
                ErrorMessage.SurveyDashboard_TemplateFilter_NotFound,
                ErrorType.NotFound));
        }

        if (template is not null && request.Source != SurveyDashboardSource.All && request.Source != template.DashboardSource)
        {
            return TemplateResult.Fail(new Error(
                "Reports.SurveyResponses.TemplateFilterSourceMismatch",
                ErrorMessage.SurveyDashboard_TemplateFilter_SourceMismatch,
                ErrorType.Validation));
        }

        return TemplateResult.Ok(template);
    }

    private static PeriodResult ResolvePeriod(GetSurveyResponsesPaginationQuery request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = request.To.HasValue ? DateOnly.FromDateTime(request.To.Value) : today;
        var from = request.From.HasValue ? DateOnly.FromDateTime(request.From.Value) : to.AddDays(-DefaultPeriodDays);

        return from > to
            ? PeriodResult.Fail(new Error(
                "Reports.SurveyResponses.DateRangeInvalid",
                ErrorMessage.GetSurveyDashboard_DateRange_Invalid,
                ErrorType.Validation))
            : PeriodResult.Ok(from, to);
    }

    private List<SurveyDashboardResponseRow> ApplyCalculatedScores(
        IReadOnlyCollection<SurveyDashboardResponseRow> responses,
        IReadOnlyCollection<SurveyDashboardAnswerRow> answers,
        IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
        IReadOnlyCollection<ConditionFlatDto> conditions,
        IReadOnlyCollection<QuestionOptionFlatDto> questionOptions,
        CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport.ScoreCalculationMode mode)
    {
        var scoreResults = _scoringService.CalculateResponseScores(
            responses.Select(x => new ResponseFlatDto
            {
                ResponseId = x.ResponseId,
                TemplateId = x.TemplateId,
                TemplateKind = x.Source == SurveyDashboardSource.Anonymous
                    ? ReportTemplateKind.Anonymous
                    : ReportTemplateKind.Normal,
                SubmittedOnUtc = x.SubmittedOnUtc
            }).ToArray(),
            answers.Select(x => new AnswerFlatDto
            {
                ResponseId = x.ResponseId,
                TemplateId = x.TemplateId,
                QuestionId = x.QuestionId,
                QuestionType = x.QuestionType,
                SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                StarRatingValue = x.StarRatingValue,
                SmileValue = x.SmileValue,
                HasTextAnswer = !string.IsNullOrWhiteSpace(x.TextAnswer),
                HasVoiceAnswer = !string.IsNullOrWhiteSpace(x.VoiceFileName)
            }).ToArray(),
            templateQuestions,
            conditions,
            questionOptions,
            mode);

        var scores = scoreResults.ToDictionary(
            x => (x.TemplateKind == ReportTemplateKind.Anonymous ? SurveyDashboardSource.Anonymous : SurveyDashboardSource.Internal, x.ResponseId));

        return responses.Select(response =>
        {
            if (!scores.TryGetValue((response.Source, response.ResponseId), out var score))
            {
                return response with { MaxScore = 0, ScorePercentage = 0m };
            }

            return response with { MaxScore = score.ScoredItemsCount, ScorePercentage = score.ScorePercentage };
        }).ToList();
    }

    private static List<SurveyDashboardResponseRow> FilterResponses(
        GetSurveyResponsesPaginationQuery query,
        IReadOnlyCollection<SurveyDashboardResponseRow> responses,
        IReadOnlyCollection<SurveyDashboardAnswerRow> answers,
        IReadOnlyCollection<SurveyDashboardCustomInputValueRow> customInputs)
    {
        IEnumerable<SurveyDashboardResponseRow> filtered = responses;

        if (query.IsScored.HasValue)
        {
            filtered = filtered.Where(x => (x.MaxScore > 0) == query.IsScored.Value);
        }

        if (query.SatisfactionCategory.HasValue)
        {
            filtered = filtered.Where(x => SatisfactionCategoryRule.Matches(
                x.MaxScore, x.ScorePercentage, query.SatisfactionCategory.Value));
        }

        if (query.HasComplaint.HasValue)
        {
            filtered = filtered.Where(x => x.HasComplaint == query.HasComplaint.Value);
        }

        if (query.HasVoice.HasValue)
        {
            filtered = filtered.Where(x => x.HasVoice == query.HasVoice.Value);
        }

        if (query.QuestionId.HasValue)
        {
            var ids = answers.Where(x => x.QuestionId == query.QuestionId.Value)
                .Select(x => (x.Source, x.ResponseId)).ToHashSet();
            filtered = filtered.Where(x => ids.Contains((x.Source, x.ResponseId)));
        }

        if (HasCustomInputFilter(query))
        {
            var ids = customInputs.Where(x => MatchesCustomInput(query, x))
                .Select(x => (x.Source, x.ResponseId)).ToHashSet();
            filtered = filtered.Where(x => ids.Contains((x.Source, x.ResponseId)));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var search = query.SearchText.Trim();
            var customIds = customInputs.Where(x =>
                    x.NameSnapshot.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(x.StringValue) && x.StringValue.Contains(search, StringComparison.OrdinalIgnoreCase)))
                .Select(x => (x.Source, x.ResponseId)).ToHashSet();

            filtered = filtered.Where(x =>
                x.TemplateNameEn.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.TemplateNameAr?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                x.BranchNameEn.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.BranchNameAr?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.OperatorNameEn?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.OperatorNameAr?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                customIds.Contains((x.Source, x.ResponseId)));
        }

        return filtered.ToList();
    }

    private static bool HasCustomInputFilter(GetSurveyResponsesPaginationQuery query)
        => !string.IsNullOrWhiteSpace(query.CustomInputName) ||
           !string.IsNullOrWhiteSpace(query.CustomInputLabelEn) ||
           !string.IsNullOrWhiteSpace(query.CustomInputLabelAr) ||
           query.CustomInputLabelEnIsNull.HasValue ||
           query.CustomInputLabelArIsNull.HasValue ||
           query.CustomInputType.HasValue ||
           !string.IsNullOrWhiteSpace(query.CustomInputValue);

    private static bool MatchesCustomInput(
        GetSurveyResponsesPaginationQuery query,
        SurveyDashboardCustomInputValueRow value)
    {
        if (!string.IsNullOrWhiteSpace(query.CustomInputName) && value.NameSnapshot != query.CustomInputName.Trim()) return false;
        if (!string.IsNullOrWhiteSpace(query.CustomInputLabelEn) && value.LabelEn != query.CustomInputLabelEn.Trim()) return false;
        if (!string.IsNullOrWhiteSpace(query.CustomInputLabelAr) && value.LabelAr != query.CustomInputLabelAr.Trim()) return false;
        if (query.CustomInputLabelEnIsNull.HasValue && (value.LabelEn is null) != query.CustomInputLabelEnIsNull.Value) return false;
        if (query.CustomInputLabelArIsNull.HasValue && (value.LabelAr is null) != query.CustomInputLabelArIsNull.Value) return false;
        if (query.CustomInputType.HasValue && value.TypeSnapshot != query.CustomInputType.Value) return false;

        if (string.IsNullOrWhiteSpace(query.CustomInputValue)) return true;
        var requested = query.CustomInputValue.Trim();
        return value.TypeSnapshot switch
        {
            TemplateCustomInputType.Integer => int.TryParse(requested, out var number) && value.IntegerValue == number,
            TemplateCustomInputType.String => value.StringValue?.Trim() == requested,
            _ => false
        };
    }

    private static SurveyResponsePaginationItemResponse MapItem(
        SurveyDashboardResponseRow row,
        IReadOnlyCollection<SurveyResponseCustomInputPreviewResponse> previews)
        => new()
        {
            Source = row.Source,
            ResponseId = row.ResponseId,
            TemplateId = row.TemplateId,
            TemplateNameEn = row.TemplateNameEn,
            TemplateNameAr = row.TemplateNameAr,
            BranchId = row.BranchId,
            BranchNameEn = row.BranchNameEn,
            BranchNameAr = row.BranchNameAr,
            OperatorId = row.OperatorId,
            OperatorNameEn = row.OperatorNameEn,
            OperatorNameAr = row.OperatorNameAr,
            SubmittedOnUtc = row.SubmittedOnUtc,
            ScorePercentage = row.ScorePercentage,
            IsScored = row.MaxScore > 0,
            HasComplaint = row.HasComplaint,
            HasVoice = row.HasVoice,
            CustomInputsPreview = previews,
            DetailsNavigation = row.Source == SurveyDashboardSource.Anonymous
                ? DashboardDrillDownPathBuilder.Navigation(
                    "AnonymousResponseDetails",
                    $"/api/anonymous-templates/{row.TemplateId}/responses/{row.ResponseId}")
                : DashboardDrillDownPathBuilder.Navigation(
                    "InternalResponseDetails",
                    $"/api/reports/branch-responses/{row.ResponseId}")
        };

    private static SurveyResponseCustomInputPreviewResponse MapPreview(SurveyDashboardCustomInputValueRow value)
        => new()
        {
            Name = value.NameSnapshot,
            LabelEn = value.LabelEn,
            LabelAr = value.LabelAr,
            Value = value.TypeSnapshot switch
            {
                TemplateCustomInputType.String => value.StringValue?.Trim() ?? string.Empty,
                TemplateCustomInputType.Integer => value.IntegerValue?.ToString() ?? string.Empty,
                _ => string.Empty
            }
        };

    private static Result<Pagination<SurveyResponsePaginationItemResponse>> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<Pagination<SurveyResponsePaginationItemResponse>>.Fail(new Error(code, message, type));

    private sealed record ScopeResult(Guid? BranchId, Error? Error)
    {
        public static ScopeResult Ok(Guid? branchId) => new(branchId, null);
        public static ScopeResult Fail(Error error) => new(null, error);
    }

    private sealed record TemplateResult(ResolvedSurveyDashboardTemplateFilter? Template, Error? Error)
    {
        public static TemplateResult Ok(ResolvedSurveyDashboardTemplateFilter? template) => new(template, null);
        public static TemplateResult Fail(Error error) => new(null, error);
    }

    private sealed record PeriodResult(DateOnly? From, DateOnly? To, Error? Error)
    {
        public static PeriodResult Ok(DateOnly from, DateOnly to) => new(from, to, null);
        public static PeriodResult Fail(Error error) => new(null, null, error);
    }
}
