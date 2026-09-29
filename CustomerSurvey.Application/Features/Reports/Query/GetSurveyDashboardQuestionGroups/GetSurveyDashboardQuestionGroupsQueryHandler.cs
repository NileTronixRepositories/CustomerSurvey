using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardQuestionGroups;

internal sealed class GetSurveyDashboardQuestionGroupsQueryHandler
    : IQueryHandler<GetSurveyDashboardQuestionGroupsQuery, SurveyDashboardQuestionGroupsResponse>
{
    private readonly ISurveyDashboardRequestResolver _requestResolver;
    private readonly IWriteReadRepository<Template> _templateRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerRepository;
    private readonly IWriteReadRepository<AnonymousSurveyAnswer> _anonymousSurveyAnswerRepository;
    private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionRepository;
    private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionRepository;
    private readonly IWriteReadRepository<TemplateQuestionCondition> _conditionRepository;
    private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _anonymousConditionRepository;
    private readonly IWriteReadRepository<QuestionOption> _questionOptionRepository;
    private readonly ISurveyReportScoringService _scoringService;

    public GetSurveyDashboardQuestionGroupsQueryHandler(
        ISurveyDashboardRequestResolver requestResolver,
        IWriteReadRepository<Template> templateRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseRepository,
        IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerRepository,
        IWriteReadRepository<AnonymousSurveyAnswer> anonymousSurveyAnswerRepository,
        IWriteReadRepository<TemplateQuestion> templateQuestionRepository,
        IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionRepository,
        IWriteReadRepository<TemplateQuestionCondition> conditionRepository,
        IWriteReadRepository<AnonymousTemplateQuestionCondition> anonymousConditionRepository,
        IWriteReadRepository<QuestionOption> questionOptionRepository,
        ISurveyReportScoringService scoringService)
    {
        _requestResolver = requestResolver;
        _templateRepository = templateRepository;
        _anonymousTemplateRepository = anonymousTemplateRepository;
        _surveyResponseRepository = surveyResponseRepository;
        _anonymousSurveyResponseRepository = anonymousSurveyResponseRepository;
        _surveyAnswerRepository = surveyAnswerRepository;
        _anonymousSurveyAnswerRepository = anonymousSurveyAnswerRepository;
        _templateQuestionRepository = templateQuestionRepository;
        _anonymousTemplateQuestionRepository = anonymousTemplateQuestionRepository;
        _conditionRepository = conditionRepository;
        _anonymousConditionRepository = anonymousConditionRepository;
        _questionOptionRepository = questionOptionRepository;
        _scoringService = scoringService;
    }

    public async Task<Result<SurveyDashboardQuestionGroupsResponse>> Handle(
        GetSurveyDashboardQuestionGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var resolution = await _requestResolver.ResolveAsync(request, cancellationToken);
        if (resolution.IsFailure)
        {
            return Result<SurveyDashboardQuestionGroupsResponse>.Fail(resolution.Errors);
        }

        var context = resolution.Value;
        var fromUtc = context.Period.From.ToDateTime(TimeOnly.MinValue);
        var toExclusiveUtc = context.Period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var templates = new List<SurveyDashboardTemplateRow>();
        var responses = new List<SurveyDashboardResponseRow>();
        var answers = new List<SurveyDashboardAnswerRow>();
        var templateQuestions = new List<TemplateQuestionFlatDto>();
        var conditions = new List<ConditionFlatDto>();

        if (context.IncludeInternal)
        {
            var currentTemplates = await _templateRepository.ListAsync(
                new GetSurveyDashboardTemplatesSpec(context.Scope.BranchId, context.InternalTemplateId),
                cancellationToken);
            templates.AddRange(currentTemplates);
            responses.AddRange(await _surveyResponseRepository.ListAsync(
                new GetSurveyDashboardInternalResponsesSpec(context.Scope.BranchId, fromUtc, toExclusiveUtc, context.InternalTemplateId),
                cancellationToken));
            answers.AddRange(await _surveyAnswerRepository.ListAsync(
                new GetSurveyDashboardInternalAnswersSpec(context.Scope.BranchId, fromUtc, toExclusiveUtc, context.InternalTemplateId),
                cancellationToken));

            var ids = currentTemplates.Select(x => x.TemplateId).ToArray();
            if (ids.Length > 0)
            {
                templateQuestions.AddRange(await _templateQuestionRepository.ListAsync(
                    new GetSurveyDashboardTemplateQuestionsSpec(ids), cancellationToken));
                conditions.AddRange(await _conditionRepository.ListAsync(
                    new GetSurveyDashboardTemplateQuestionConditionsSpec(ids), cancellationToken));
            }
        }

        if (context.IncludeAnonymous)
        {
            var currentTemplates = await _anonymousTemplateRepository.ListAsync(
                new GetSurveyDashboardAnonymousTemplatesSpec(context.Scope.BranchId, context.AnonymousTemplateId),
                cancellationToken);
            templates.AddRange(currentTemplates);
            responses.AddRange(await _anonymousSurveyResponseRepository.ListAsync(
                new GetSurveyDashboardAnonymousResponsesSpec(context.Scope.BranchId, fromUtc, toExclusiveUtc, context.AnonymousTemplateId),
                cancellationToken));
            answers.AddRange(await _anonymousSurveyAnswerRepository.ListAsync(
                new GetSurveyDashboardAnonymousAnswersSpec(context.Scope.BranchId, fromUtc, toExclusiveUtc, context.AnonymousTemplateId),
                cancellationToken));

            var ids = currentTemplates.Select(x => x.TemplateId).ToArray();
            if (ids.Length > 0)
            {
                templateQuestions.AddRange(await _anonymousTemplateQuestionRepository.ListAsync(
                    new GetSurveyDashboardAnonymousTemplateQuestionsSpec(ids), cancellationToken));
                conditions.AddRange(await _anonymousConditionRepository.ListAsync(
                    new GetSurveyDashboardAnonymousTemplateQuestionConditionsSpec(ids), cancellationToken));
            }
        }

        var questionIds = templateQuestions.Select(x => x.QuestionId).Distinct().ToArray();
        IReadOnlyCollection<QuestionOptionFlatDto> options = questionIds.Length == 0
            ? Array.Empty<QuestionOptionFlatDto>()
            : await _questionOptionRepository.ListAsync(
                new GetSurveyDashboardQuestionOptionsSpec(questionIds), cancellationToken);

        var responseDtos = responses.Select(x => new ResponseFlatDto
        {
            ResponseId = x.ResponseId,
            TemplateId = x.TemplateId,
            TemplateKind = ToReportKind(x.Source),
            SubmittedOnUtc = x.SubmittedOnUtc
        }).ToArray();

        var answerDtos = answers.Select(x => new AnswerFlatDto
        {
            ResponseId = x.ResponseId,
            TemplateKind = ToReportKind(x.Source),
            TemplateId = x.TemplateId,
            QuestionId = x.QuestionId,
            QuestionType = x.QuestionType,
            SelectedQuestionOptionId = x.SelectedQuestionOptionId,
            StarRatingValue = x.StarRatingValue,
            SmileValue = x.SmileValue,
            HasTextAnswer = !string.IsNullOrWhiteSpace(x.TextAnswer),
            HasVoiceAnswer = !string.IsNullOrWhiteSpace(x.VoiceFileName)
        }).ToArray();

        var tokens = _scoringService.CalculateQuestionScoreTokens(
            responseDtos,
            answerDtos,
            templateQuestions,
            conditions,
            options,
            request.ScoreCalculationMode);

        var templatesByKey = templates.ToDictionary(
            x => (ToReportKind(x.Source), x.TemplateId),
            x => x);
        var totalResponsesByTemplate = responses
            .GroupBy(x => (ToReportKind(x.Source), x.TemplateId))
            .ToDictionary(x => x.Key, x => x.Count());
        var tokensByQuestion = tokens
            .GroupBy(x => (x.TemplateKind, x.TemplateId, x.TemplateQuestionId))
            .ToDictionary(x => x.Key, x => x.ToArray());

        var groups = templateQuestions
            .GroupBy(x => new
            {
                x.TemplateKind,
                x.TemplateId,
                x.QuestionGroupId,
                x.QuestionGroupNameEn,
                x.QuestionGroupNameAr
            })
            .Select(group =>
            {
                var groupTokens = group
                    .SelectMany(question => tokensByQuestion.GetValueOrDefault(
                        (question.TemplateKind, question.TemplateId, question.TemplateQuestionId),
                        Array.Empty<QuestionScoreToken>()))
                    .ToArray();
                var templateKey = (group.Key.TemplateKind, group.Key.TemplateId);
                var average = groupTokens.Length == 0
                    ? (decimal?)null
                    : ReportScoreRounding.Round(groupTokens.Average(x => x.ScoreValue));
                var template = templatesByKey[templateKey];

                return new SurveyDashboardQuestionGroupItemResponse
                {
                    TemplateId = group.Key.TemplateId,
                    TemplateKind = group.Key.TemplateKind == ReportTemplateKind.Anonymous
                        ? SurveyDashboardTemplateKind.Anonymous
                        : SurveyDashboardTemplateKind.Authorized,
                    TemplateNameEn = template.TemplateNameEn,
                    TemplateNameAr = template.TemplateNameAr,
                    QuestionGroupId = group.Key.QuestionGroupId,
                    QuestionGroupNameEn = group.Key.QuestionGroupNameEn,
                    QuestionGroupNameAr = group.Key.QuestionGroupNameAr,
                    QuestionsCount = group.Count(),
                    ScorableQuestionsCount = group.Count(x => IsScorable(x.QuestionType)),
                    TotalResponses = totalResponsesByTemplate.GetValueOrDefault(templateKey),
                    ScoredResponsesCount = groupTokens.Select(x => x.ResponseId).Distinct().Count(),
                    ScoredItemsCount = groupTokens.Length,
                    AverageScoreValue = average,
                    AverageScorePercentage = average.HasValue
                        ? ReportScoreRounding.Round(average.Value / 5m * 100m)
                        : null
                };
            })
            .OrderBy(x => x.TemplateNameEn)
            .ThenByDescending(x => x.AverageScorePercentage)
            .ThenBy(x => x.QuestionGroupNameEn)
            .ToArray();

        return Result<SurveyDashboardQuestionGroupsResponse>.Ok(new SurveyDashboardQuestionGroupsResponse
        {
            AppliedFilters = new SurveyDashboardAppliedFiltersResponse
            {
                BranchId = context.Scope.BranchId,
                Source = context.AppliedSource,
                TemplateId = context.TemplateFilter?.TemplateId,
                TemplateKind = context.TemplateFilter?.TemplateKind,
                From = context.Period.From,
                To = context.Period.To,
                ScoreCalculationMode = request.ScoreCalculationMode
            },
            QuestionGroups = groups
        });
    }

    private static ReportTemplateKind ToReportKind(SurveyDashboardSource source)
        => source == SurveyDashboardSource.Anonymous ? ReportTemplateKind.Anonymous : ReportTemplateKind.Normal;

    private static bool IsScorable(QuestionType type)
        => type is QuestionType.SingleChoice or QuestionType.StarRating or QuestionType.Smiles;
}
