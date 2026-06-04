using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Reports
{
    internal sealed class BranchTemplatesPdfReportService : IBranchTemplatesPdfReportService
    {
        private const decimal MaxScoreValue = 5m;

        private readonly IWriteReadRepository<Branch> _branchRepository;
        private readonly IWriteReadRepository<Template> _templateRepository;
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateRepository;
        private readonly IWriteReadRepository<SurveyResponse> _surveyResponseRepository;
        private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerRepository;
        private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseRepository;
        private readonly IWriteReadRepository<AnonymousSurveyAnswer> _anonymousSurveyAnswerRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _templateQuestionConditionRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _anonymousTemplateQuestionConditionRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionRepository;
        private readonly IPdfService _pdfService;

        public BranchTemplatesPdfReportService(
            IWriteReadRepository<Branch> branchRepository,
            IWriteReadRepository<Template> templateRepository,
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateRepository,
            IWriteReadRepository<SurveyResponse> surveyResponseRepository,
            IWriteReadRepository<SurveyAnswer> surveyAnswerRepository,
            IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseRepository,
            IWriteReadRepository<AnonymousSurveyAnswer> anonymousSurveyAnswerRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionRepository,
            IWriteReadRepository<TemplateQuestionCondition> templateQuestionConditionRepository,
            IWriteReadRepository<AnonymousTemplateQuestionCondition> anonymousTemplateQuestionConditionRepository,
            IWriteReadRepository<QuestionOption> questionOptionRepository,
            IPdfService pdfService)
        {
            _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _anonymousTemplateRepository = anonymousTemplateRepository ?? throw new ArgumentNullException(nameof(anonymousTemplateRepository));
            _surveyResponseRepository = surveyResponseRepository ?? throw new ArgumentNullException(nameof(surveyResponseRepository));
            _surveyAnswerRepository = surveyAnswerRepository ?? throw new ArgumentNullException(nameof(surveyAnswerRepository));
            _anonymousSurveyResponseRepository = anonymousSurveyResponseRepository ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseRepository));
            _anonymousSurveyAnswerRepository = anonymousSurveyAnswerRepository ?? throw new ArgumentNullException(nameof(anonymousSurveyAnswerRepository));
            _templateQuestionRepository = templateQuestionRepository ?? throw new ArgumentNullException(nameof(templateQuestionRepository));
            _anonymousTemplateQuestionRepository = anonymousTemplateQuestionRepository ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionRepository));
            _templateQuestionConditionRepository = templateQuestionConditionRepository ?? throw new ArgumentNullException(nameof(templateQuestionConditionRepository));
            _anonymousTemplateQuestionConditionRepository = anonymousTemplateQuestionConditionRepository ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionConditionRepository));
            _questionOptionRepository = questionOptionRepository ?? throw new ArgumentNullException(nameof(questionOptionRepository));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        }

        public async Task<Result<BranchTemplatesPdfReportFile>> GenerateAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
        {
            var isArabic = string.Equals(
                request.Language,
                "ar",
                StringComparison.OrdinalIgnoreCase);

            var modelResult = await BuildReportModelAsync(
                request,
                cancellationToken);

            if (modelResult.IsFailure)
            {
                return Result<BranchTemplatesPdfReportFile>.Fail(modelResult.Errors);
            }

            var pdfBytes = await _pdfService.GeneratePdfAsync(
                viewName: "BranchTemplatesPdfReport",
                model: modelResult.Value,
                options: new PdfRenderOptions
                {
                    IsDraft = false,
                    PrintBackground = true,
                    DisplayHeaderFooter = false,
                    IsLandscape = false,
                    Format = "A4",
                    MarginTopCm = 0.7m,
                    MarginRightCm = 0.7m,
                    MarginBottomCm = 0.7m,
                    MarginLeftCm = 0.7m
                },
                cancellationToken);

            var fileName = isArabic
                ? $"تقرير-استبيانات-العملاء-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
                : $"customer-survey-executive-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return Result<BranchTemplatesPdfReportFile>.Ok(
                new BranchTemplatesPdfReportFile
                {
                    FileName = fileName,
                    ContentType = "application/pdf",
                    Content = pdfBytes
                });
        }

        public async Task<Result<BranchTemplatesPdfReportModel>> BuildReportModelAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
        {
            var isArabic = string.Equals(
                request.Language,
                "ar",
                StringComparison.OrdinalIgnoreCase);

            var fromUtc = request.FromDate.ToDateTime(TimeOnly.MinValue);
            var toExclusiveUtc = request.ToDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var branch = await _branchRepository.Query()
                .Where(x => x.Id == request.BranchId)
                .Select(x => new
                {
                    x.Id,
                    x.NameEn,
                    x.NameAr
                })
                .FirstAsync(cancellationToken);

            var normalTemplates = await LoadNormalTemplatesAsync(request, cancellationToken);
            var anonymousTemplates = await LoadAnonymousTemplatesAsync(request, cancellationToken);

            var selectionError = ValidateSelectedTemplateResolution(
                request,
                normalTemplates,
                anonymousTemplates);

            if (selectionError is not null)
            {
                return Result<BranchTemplatesPdfReportModel>.Fail(selectionError);
            }

            var normalTemplateIds = normalTemplates.Select(x => x.TemplateId).ToArray();
            var anonymousTemplateIds = anonymousTemplates.Select(x => x.TemplateId).ToArray();

            var normalResponses = await LoadNormalResponsesAsync(
                normalTemplateIds,
                fromUtc,
                toExclusiveUtc,
                cancellationToken);

            var anonymousResponses = await LoadAnonymousResponsesAsync(
                anonymousTemplateIds,
                fromUtc,
                toExclusiveUtc,
                cancellationToken);

            var normalQuestionBuild = await LoadNormalQuestionAnalyticsAsync(
                normalTemplates,
                normalResponses,
                request.ScoreCalculationMode,
                cancellationToken);

            var anonymousQuestionBuild = await LoadAnonymousQuestionAnalyticsAsync(
                anonymousTemplates,
                anonymousResponses,
                request.ScoreCalculationMode,
                cancellationToken);

            var templates = BuildTemplateSummaries(
                normalTemplates,
                anonymousTemplates,
                normalResponses,
                anonymousResponses,
                normalQuestionBuild.Questions,
                anonymousQuestionBuild.Questions,
                normalQuestionBuild.ScoreTokens,
                anonymousQuestionBuild.ScoreTokens);

            var allQuestions = normalQuestionBuild.Questions
                .Concat(anonymousQuestionBuild.Questions)
                .OrderBy(x => x.TemplateKind)
                .ThenBy(x => x.TemplateNameEn)
                .ThenByDescending(x => x.IsRootQuestion)
                .ThenBy(x => x.QuestionTextEn)
                .ToArray();

            var allFlowLines = normalQuestionBuild.FlowLines
                .Concat(anonymousQuestionBuild.FlowLines)
                .ToArray();

            var allScoreTokens = normalQuestionBuild.ScoreTokens
                .Concat(anonymousQuestionBuild.ScoreTokens)
                .ToArray();

            var executiveSummary = BuildExecutiveSummary(
                templates,
                normalResponses,
                anonymousResponses,
                allQuestions,
                allScoreTokens,
                isArabic);

            var templateDetails = BuildTemplateDetails(
                templates,
                allQuestions,
                allFlowLines);

            var model = new BranchTemplatesPdfReportModel
            {
                Language = request.Language,
                BranchName = isArabic && !string.IsNullOrWhiteSpace(branch.NameAr)
                    ? branch.NameAr!
                    : branch.NameEn,
                GeneratedBy = request.GeneratedByName,
                GeneratedAtUtc = DateTime.UtcNow,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                SelectedTemplateId = request.TemplateId,
                SelectedTemplateKind = ResolveSelectedTemplateKind(
                    request,
                    normalTemplates,
                    anonymousTemplates),
                SelectedTemplateName = ResolveSelectedTemplateName(
                    request,
                    normalTemplates,
                    anonymousTemplates,
                    isArabic),
                ScoreCalculationMode = request.ScoreCalculationMode,
                TopWorstQuestionsCount = request.TopWorstQuestionsCount,
                ExecutiveSummary = executiveSummary,
                Templates = templates,
                Questions = allQuestions,
                WorstQuestions = BuildQuestionRanks(
                    allQuestions,
                    request.TopWorstQuestionsCount,
                    worst: true),
                BestQuestions = BuildQuestionRanks(
                    allQuestions,
                    request.TopWorstQuestionsCount,
                    worst: false),
                TemplateDetails = templateDetails
            };

            return Result<BranchTemplatesPdfReportModel>.Ok(model);
        }

        private async Task<IReadOnlyCollection<TemplateHeaderDto>> LoadNormalTemplatesAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
        {
            if (request.TemplateKind == ReportTemplateKind.Anonymous)
            {
                return Array.Empty<TemplateHeaderDto>();
            }

            var query = _templateRepository.Query()
                .Where(x => x.BranchId == request.BranchId);

            if (request.TemplateId.HasValue)
            {
                query = query.Where(x => x.Id == request.TemplateId.Value);
            }

            return await query
                .Select(x => new TemplateHeaderDto
                {
                    TemplateId = x.Id,
                    TemplateKind = ReportTemplateKind.Normal,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    Status = x.Status.ToString(),
                    ActiveFrom = x.ActiveFrom,
                    ExpireTo = x.ExpireTo
                })
                .ToArrayAsync(cancellationToken);
        }

        private async Task<IReadOnlyCollection<TemplateHeaderDto>> LoadAnonymousTemplatesAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
        {
            if (request.TemplateKind == ReportTemplateKind.Normal)
            {
                return Array.Empty<TemplateHeaderDto>();
            }

            var query = _anonymousTemplateRepository.Query()
                .Where(x =>
                    x.BranchId.HasValue &&
                    x.BranchId.Value == request.BranchId);

            if (request.TemplateId.HasValue)
            {
                query = query.Where(x => x.Id == request.TemplateId.Value);
            }

            return await query
                .Select(x => new TemplateHeaderDto
                {
                    TemplateId = x.Id,
                    TemplateKind = ReportTemplateKind.Anonymous,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    Status = x.Status.ToString(),
                    ActiveFrom = x.ActiveFrom,
                    ExpireTo = x.ExpireTo
                })
                .ToArrayAsync(cancellationToken);
        }

        private async Task<IReadOnlyCollection<ResponseFlatDto>> LoadNormalResponsesAsync(
            IReadOnlyCollection<Guid> templateIds,
            DateTime fromUtc,
            DateTime toExclusiveUtc,
            CancellationToken cancellationToken)
        {
            if (templateIds.Count == 0)
            {
                return Array.Empty<ResponseFlatDto>();
            }

            return await _surveyResponseRepository.Query()
                .Where(x =>
                    templateIds.Contains(x.TemplateId) &&
                    x.SubmittedOnUtc >= fromUtc &&
                    x.SubmittedOnUtc < toExclusiveUtc)
                .Select(x => new ResponseFlatDto
                {
                    ResponseId = x.Id,
                    TemplateId = x.TemplateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    SubmittedOnUtc = x.SubmittedOnUtc
                })
                .ToArrayAsync(cancellationToken);
        }

        private async Task<IReadOnlyCollection<ResponseFlatDto>> LoadAnonymousResponsesAsync(
            IReadOnlyCollection<Guid> templateIds,
            DateTime fromUtc,
            DateTime toExclusiveUtc,
            CancellationToken cancellationToken)
        {
            if (templateIds.Count == 0)
            {
                return Array.Empty<ResponseFlatDto>();
            }

            return await _anonymousSurveyResponseRepository.Query()
                .Where(x =>
                    templateIds.Contains(x.AnonymousTemplateId) &&
                    x.SubmittedOnUtc >= fromUtc &&
                    x.SubmittedOnUtc < toExclusiveUtc)
                .Select(x => new ResponseFlatDto
                {
                    ResponseId = x.Id,
                    TemplateId = x.AnonymousTemplateId,
                    TemplateKind = ReportTemplateKind.Anonymous,
                    SubmittedOnUtc = x.SubmittedOnUtc
                })
                .ToArrayAsync(cancellationToken);
        }

        private async Task<QuestionAnalyticsBuildResult> LoadNormalQuestionAnalyticsAsync(
            IReadOnlyCollection<TemplateHeaderDto> templates,
            IReadOnlyCollection<ResponseFlatDto> responses,
            ScoreCalculationMode scoreCalculationMode,
            CancellationToken cancellationToken)
        {
            var templateIds = templates.Select(x => x.TemplateId).ToArray();

            if (templateIds.Length == 0)
            {
                return QuestionAnalyticsBuildResult.Empty();
            }

            var templateQuestions = await _templateQuestionRepository.Query()
                .Where(x => templateIds.Contains(x.TemplateId))
                .Select(x => new TemplateQuestionFlatDto
                {
                    TemplateQuestionId = x.Id,
                    TemplateId = x.TemplateId,
                    QuestionId = x.QuestionId,
                    Order = x.Order,
                    QuestionTextEn = x.Question.TextEn,
                    QuestionTextAr = x.Question.TextAr,
                    QuestionType = x.Question.Type
                })
                .ToArrayAsync(cancellationToken);

            var conditions = await _templateQuestionConditionRepository.Query()
                .Where(x => templateIds.Contains(x.TemplateId) && x.IsActive)
                .Select(x => new ConditionFlatDto
                {
                    TemplateId = x.TemplateId,
                    ParentTemplateQuestionId = x.ParentTemplateQuestionId,
                    ChildTemplateQuestionId = x.ChildTemplateQuestionId,
                    TriggerType = x.TriggerType,
                    SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                    TriggerValue = x.TriggerValue
                })
                .ToArrayAsync(cancellationToken);

            var responseIds = responses.Select(x => x.ResponseId).ToArray();

            var answers = responseIds.Length == 0
                ? Array.Empty<AnswerFlatDto>()
                : AttachTemplateIds(
                    await _surveyAnswerRepository.Query()
                        .Where(x => responseIds.Contains(x.SurveyResponseId))
                        .Select(x => new AnswerFlatDto
                        {
                            ResponseId = x.SurveyResponseId,
                            QuestionId = x.QuestionId,
                            QuestionType = x.QuestionType,
                            SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                            StarRatingValue = x.StarRatingValue,
                            SmileValue = x.SmileValue,
                            HasTextAnswer = !string.IsNullOrWhiteSpace(x.TextAnswer),
                            HasVoiceAnswer = !string.IsNullOrWhiteSpace(x.VoiceFileName)
                        })
                        .ToArrayAsync(cancellationToken),
                    responses);

            return await BuildQuestionAnalyticsAsync(
                templates,
                templateQuestions,
                conditions,
                responses,
                answers,
                ReportTemplateKind.Normal,
                scoreCalculationMode,
                cancellationToken);
        }

        private async Task<QuestionAnalyticsBuildResult> LoadAnonymousQuestionAnalyticsAsync(
            IReadOnlyCollection<TemplateHeaderDto> templates,
            IReadOnlyCollection<ResponseFlatDto> responses,
            ScoreCalculationMode scoreCalculationMode,
            CancellationToken cancellationToken)
        {
            var templateIds = templates.Select(x => x.TemplateId).ToArray();

            if (templateIds.Length == 0)
            {
                return QuestionAnalyticsBuildResult.Empty();
            }

            var templateQuestions = await _anonymousTemplateQuestionRepository.Query()
                .Where(x => templateIds.Contains(x.AnonymousTemplateId))
                .Select(x => new TemplateQuestionFlatDto
                {
                    TemplateQuestionId = x.Id,
                    TemplateId = x.AnonymousTemplateId,
                    QuestionId = x.QuestionId,
                    Order = x.Order,
                    QuestionTextEn = x.Question.TextEn,
                    QuestionTextAr = x.Question.TextAr,
                    QuestionType = x.Question.Type
                })
                .ToArrayAsync(cancellationToken);

            var conditions = await _anonymousTemplateQuestionConditionRepository.Query()
                .Where(x => templateIds.Contains(x.AnonymousTemplateId) && x.IsActive)
                .Select(x => new ConditionFlatDto
                {
                    TemplateId = x.AnonymousTemplateId,
                    ParentTemplateQuestionId = x.ParentAnonymousTemplateQuestionId,
                    ChildTemplateQuestionId = x.ChildAnonymousTemplateQuestionId,
                    TriggerType = x.TriggerType,
                    SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                    TriggerValue = x.TriggerValue
                })
                .ToArrayAsync(cancellationToken);

            var responseIds = responses.Select(x => x.ResponseId).ToArray();

            var answers = responseIds.Length == 0
                ? Array.Empty<AnswerFlatDto>()
                : AttachTemplateIds(
                    await _anonymousSurveyAnswerRepository.Query()
                        .Where(x => responseIds.Contains(x.AnonymousSurveyResponseId))
                        .Select(x => new AnswerFlatDto
                        {
                            ResponseId = x.AnonymousSurveyResponseId,
                            QuestionId = x.QuestionId,
                            QuestionType = x.QuestionType,
                            SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                            StarRatingValue = x.StarRatingValue,
                            SmileValue = x.SmileValue,
                            HasTextAnswer = !string.IsNullOrWhiteSpace(x.TextAnswer),
                            HasVoiceAnswer = !string.IsNullOrWhiteSpace(x.VoiceFileName)
                        })
                        .ToArrayAsync(cancellationToken),
                    responses);

            return await BuildQuestionAnalyticsAsync(
                templates,
                templateQuestions,
                conditions,
                responses,
                answers,
                ReportTemplateKind.Anonymous,
                scoreCalculationMode,
                cancellationToken);
        }

        private async Task<QuestionAnalyticsBuildResult> BuildQuestionAnalyticsAsync(
            IReadOnlyCollection<TemplateHeaderDto> templates,
            IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
            IReadOnlyCollection<ConditionFlatDto> conditions,
            IReadOnlyCollection<ResponseFlatDto> responses,
            IReadOnlyCollection<AnswerFlatDto> answers,
            ReportTemplateKind templateKind,
            ScoreCalculationMode scoreCalculationMode,
            CancellationToken cancellationToken)
        {
            var questionIds = templateQuestions
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            var questionOptions = questionIds.Length == 0
                ? Array.Empty<QuestionOptionFlatDto>()
                : await _questionOptionRepository.Query()
                    .Where(x => questionIds.Contains(x.QuestionId) && x.IsActive)
                    .Select(x => new QuestionOptionFlatDto
                    {
                        OptionId = x.Id,
                        QuestionId = x.QuestionId,
                        TextEn = x.TextEn,
                        TextAr = x.TextAr,
                        Value = x.Value,
                        Order = x.Order
                    })
                    .ToArrayAsync(cancellationToken);

            var flowLines = BuildFlowLines(
                templates,
                templateQuestions,
                conditions,
                questionOptions,
                templateKind);

            var childTemplateQuestionIds = conditions
                .Select(x => x.ChildTemplateQuestionId)
                .Distinct()
                .ToHashSet();

            var parentConditionByChild = conditions
                .GroupBy(x => x.ChildTemplateQuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(condition => condition.TriggerValue ?? 0).First());

            var questionOptionsById = questionOptions
                .ToDictionary(x => x.OptionId);

            var scoreTokens = CalculateScoreTokens(
                templateQuestions,
                conditions,
                responses,
                answers,
                questionOptions,
                scoreCalculationMode);

            var responsesByTemplate = responses
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var answersByQuestion = answers
                .GroupBy(x => new
                {
                    x.TemplateId,
                    x.QuestionId
                })
                .ToDictionary(
                    x => (x.Key.TemplateId, x.Key.QuestionId),
                    x => x.ToArray());

            var scoreTokensByTemplateQuestion = scoreTokens
                .GroupBy(x => x.TemplateQuestionId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var templatesById = templates.ToDictionary(x => x.TemplateId);

            var result = new List<BranchTemplatesPdfQuestionAnalytics>();

            foreach (var templateQuestion in templateQuestions
                         .OrderBy(x => x.TemplateId)
                         .ThenBy(x => x.Order))
            {
                var template = templatesById[templateQuestion.TemplateId];

                responsesByTemplate.TryGetValue(templateQuestion.TemplateId, out var templateResponses);
                templateResponses ??= Array.Empty<ResponseFlatDto>();

                answersByQuestion.TryGetValue(
                    (templateQuestion.TemplateId, templateQuestion.QuestionId),
                    out var questionAnswers);
                questionAnswers ??= Array.Empty<AnswerFlatDto>();

                scoreTokensByTemplateQuestion.TryGetValue(
                    templateQuestion.TemplateQuestionId,
                    out var questionScoreTokens);
                questionScoreTokens ??= Array.Empty<ScoredAnswerTokenDto>();

                var totalResponses = templateResponses.Length;
                var totalAnswers = questionAnswers.Length;
                var skipped = Math.Max(0, totalResponses - totalAnswers);

                var isRoot = !childTemplateQuestionIds.Contains(templateQuestion.TemplateQuestionId);

                var isScoreIncluded =
                    scoreCalculationMode == ScoreCalculationMode.RootQuestions
                        ? isRoot && IsScoredQuestionType(templateQuestion.QuestionType)
                        : questionScoreTokens.Length > 0;

                ParentTriggerText? parentTrigger = null;

                if (parentConditionByChild.TryGetValue(
                        templateQuestion.TemplateQuestionId,
                        out var parentCondition))
                {
                    parentTrigger = BuildParentTriggerText(
                        parentCondition,
                        questionOptionsById);
                }

                var scoreAverageValue = questionScoreTokens.Length == 0
                    ? null
                    : (decimal?)Math.Round(questionScoreTokens.Average(x => x.ScoreValue), 2);

                result.Add(new BranchTemplatesPdfQuestionAnalytics
                {
                    TemplateId = templateQuestion.TemplateId,
                    TemplateKind = templateKind,
                    TemplateNameEn = template.NameEn,
                    TemplateNameAr = template.NameAr,
                    TemplateQuestionId = templateQuestion.TemplateQuestionId,
                    QuestionId = templateQuestion.QuestionId,
                    QuestionTextEn = templateQuestion.QuestionTextEn,
                    QuestionTextAr = templateQuestion.QuestionTextAr,
                    QuestionType = templateQuestion.QuestionType.ToString(),
                    IsRootQuestion = isRoot,
                    ParentTriggerTextEn = parentTrigger?.TextEn,
                    ParentTriggerTextAr = parentTrigger?.TextAr,
                    TotalAnswers = totalAnswers,
                    SkippedCount = skipped,
                    AverageValue = CalculateAverageValue(
                        templateQuestion.QuestionType,
                        questionAnswers,
                        questionOptions),
                    ScoreAverageValue = scoreAverageValue,
                    IsScoreIncluded = isScoreIncluded,
                    Options = BuildOptionAnalytics(
                        templateQuestion,
                        questionOptions,
                        questionAnswers)
                });
            }

            return new QuestionAnalyticsBuildResult(
                result,
                scoreTokens,
                flowLines);
        }

        private static IReadOnlyCollection<ScoredAnswerTokenDto> CalculateScoreTokens(
            IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
            IReadOnlyCollection<ConditionFlatDto> conditions,
            IReadOnlyCollection<ResponseFlatDto> responses,
            IReadOnlyCollection<AnswerFlatDto> answers,
            IReadOnlyCollection<QuestionOptionFlatDto> questionOptions,
            ScoreCalculationMode scoreCalculationMode)
        {
            if (responses.Count == 0 || templateQuestions.Count == 0 || answers.Count == 0)
            {
                return Array.Empty<ScoredAnswerTokenDto>();
            }

            var templateQuestionsByTemplate = templateQuestions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var templateQuestionsById = templateQuestions
                .ToDictionary(x => x.TemplateQuestionId);

            var childTemplateQuestionIdsByTemplate = conditions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(c => c.ChildTemplateQuestionId).ToHashSet());

            var childConditionsByParent = conditions
                .GroupBy(x => x.ParentTemplateQuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(c => GetChildQuestionOrder(c, templateQuestionsById)).ToArray());

            var answersByResponseAndQuestion = answers
                .GroupBy(x => new
                {
                    x.ResponseId,
                    x.QuestionId
                })
                .ToDictionary(
                    x => (x.Key.ResponseId, x.Key.QuestionId),
                    x => x.First());

            var questionOptionsById = questionOptions
                .ToDictionary(x => x.OptionId);

            var result = new List<ScoredAnswerTokenDto>();

            foreach (var response in responses)
            {
                if (!templateQuestionsByTemplate.TryGetValue(response.TemplateId, out var currentTemplateQuestions))
                {
                    continue;
                }

                if (!childTemplateQuestionIdsByTemplate.TryGetValue(response.TemplateId, out var childIds))
                {
                    childIds = new HashSet<Guid>();
                }

                var rootQuestions = currentTemplateQuestions
                    .Where(x => !childIds.Contains(x.TemplateQuestionId))
                    .OrderBy(x => x.Order)
                    .ToArray();

                foreach (var rootQuestion in rootQuestions)
                {
                    if (scoreCalculationMode == ScoreCalculationMode.RootQuestions)
                    {
                        AddRootScoreTokenIfPossible(
                            response,
                            rootQuestion,
                            answersByResponseAndQuestion,
                            questionOptionsById,
                            result);

                        continue;
                    }

                    AddLowestConditionLevelScoreTokenIfPossible(
                        response,
                        rootQuestion,
                        templateQuestionsById,
                        childConditionsByParent,
                        answersByResponseAndQuestion,
                        questionOptionsById,
                        result);
                }
            }

            return result;
        }

        private static void AddRootScoreTokenIfPossible(
            ResponseFlatDto response,
            TemplateQuestionFlatDto rootQuestion,
            IReadOnlyDictionary<(Guid ResponseId, Guid QuestionId), AnswerFlatDto> answersByResponseAndQuestion,
            IReadOnlyDictionary<Guid, QuestionOptionFlatDto> questionOptionsById,
            List<ScoredAnswerTokenDto> result)
        {
            if (!answersByResponseAndQuestion.TryGetValue(
                    (response.ResponseId, rootQuestion.QuestionId),
                    out var answer))
            {
                return;
            }

            if (!TryGetScoreValue(rootQuestion.QuestionType, answer, questionOptionsById, out var scoreValue))
            {
                return;
            }

            result.Add(new ScoredAnswerTokenDto
            {
                ResponseId = response.ResponseId,
                TemplateId = response.TemplateId,
                TemplateQuestionId = rootQuestion.TemplateQuestionId,
                QuestionId = rootQuestion.QuestionId,
                ScoreValue = scoreValue
            });
        }

        private static void AddLowestConditionLevelScoreTokenIfPossible(
            ResponseFlatDto response,
            TemplateQuestionFlatDto rootQuestion,
            IReadOnlyDictionary<Guid, TemplateQuestionFlatDto> templateQuestionsById,
            IReadOnlyDictionary<Guid, ConditionFlatDto[]> childConditionsByParent,
            IReadOnlyDictionary<(Guid ResponseId, Guid QuestionId), AnswerFlatDto> answersByResponseAndQuestion,
            IReadOnlyDictionary<Guid, QuestionOptionFlatDto> questionOptionsById,
            List<ScoredAnswerTokenDto> result)
        {
            var current = rootQuestion;
            var visited = new HashSet<Guid>();

            ScoredAnswerTokenDto? lastScorableToken = null;

            while (true)
            {
                if (!visited.Add(current.TemplateQuestionId))
                {
                    break;
                }

                if (!answersByResponseAndQuestion.TryGetValue(
                        (response.ResponseId, current.QuestionId),
                        out var currentAnswer))
                {
                    break;
                }

                if (TryGetScoreValue(
                        current.QuestionType,
                        currentAnswer,
                        questionOptionsById,
                        out var scoreValue))
                {
                    lastScorableToken = new ScoredAnswerTokenDto
                    {
                        ResponseId = response.ResponseId,
                        TemplateId = response.TemplateId,
                        TemplateQuestionId = current.TemplateQuestionId,
                        QuestionId = current.QuestionId,
                        ScoreValue = scoreValue
                    };
                }

                if (!childConditionsByParent.TryGetValue(
                        current.TemplateQuestionId,
                        out var possibleConditions))
                {
                    break;
                }

                var matchedCondition = possibleConditions.FirstOrDefault(condition =>
                    IsConditionMatched(condition, currentAnswer));

                if (matchedCondition is null)
                {
                    break;
                }

                if (!templateQuestionsById.TryGetValue(
                        matchedCondition.ChildTemplateQuestionId,
                        out var childQuestion))
                {
                    break;
                }

                current = childQuestion;
            }

            if (lastScorableToken is not null)
            {
                result.Add(lastScorableToken);
            }
        }

        private static bool IsConditionMatched(
            ConditionFlatDto condition,
            AnswerFlatDto answer)
        {
            return condition.TriggerType switch
            {
                QuestionConditionTriggerType.SingleChoiceOption =>
                    condition.SelectedQuestionOptionId.HasValue &&
                    answer.SelectedQuestionOptionId.HasValue &&
                    condition.SelectedQuestionOptionId.Value == answer.SelectedQuestionOptionId.Value,

                QuestionConditionTriggerType.StarRatingValue =>
                    condition.TriggerValue.HasValue &&
                    answer.StarRatingValue.HasValue &&
                    condition.TriggerValue.Value == answer.StarRatingValue.Value,

                QuestionConditionTriggerType.SmileValue =>
                    condition.TriggerValue.HasValue &&
                    answer.SmileValue.HasValue &&
                    condition.TriggerValue.Value == answer.SmileValue.Value,

                _ => false
            };
        }

        private static bool TryGetScoreValue(
            QuestionType questionType,
            AnswerFlatDto answer,
            IReadOnlyDictionary<Guid, QuestionOptionFlatDto> questionOptionsById,
            out decimal scoreValue)
        {
            scoreValue = 0;

            if (questionType == QuestionType.StarRating && answer.StarRatingValue.HasValue)
            {
                scoreValue = answer.StarRatingValue.Value;
                return true;
            }

            if (questionType == QuestionType.Smiles && answer.SmileValue.HasValue)
            {
                scoreValue = answer.SmileValue.Value;
                return true;
            }

            if (questionType == QuestionType.SingleChoice &&
                answer.SelectedQuestionOptionId.HasValue &&
                questionOptionsById.TryGetValue(answer.SelectedQuestionOptionId.Value, out var option))
            {
                scoreValue = option.Value;
                return true;
            }

            return false;
        }

        private static IReadOnlyCollection<BranchTemplatesPdfFlowLine> BuildFlowLines(
            IReadOnlyCollection<TemplateHeaderDto> templates,
            IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
            IReadOnlyCollection<ConditionFlatDto> conditions,
            IReadOnlyCollection<QuestionOptionFlatDto> questionOptions,
            ReportTemplateKind templateKind)
        {
            var result = new List<BranchTemplatesPdfFlowLine>();

            var templateQuestionsByTemplate = templateQuestions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.OrderBy(q => q.Order).ToArray());

            var templateQuestionsById = templateQuestions
                .ToDictionary(x => x.TemplateQuestionId);

            var childTemplateQuestionIds = conditions
                .Select(x => x.ChildTemplateQuestionId)
                .ToHashSet();

            var conditionsByParent = conditions
                .GroupBy(x => x.ParentTemplateQuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(c => GetChildQuestionOrder(c, templateQuestionsById)).ToArray());

            var optionsByQuestion = questionOptions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(x => x.Key, x => x.OrderBy(option => option.Order).ToArray());

            foreach (var template in templates.OrderBy(x => x.NameEn))
            {
                if (!templateQuestionsByTemplate.TryGetValue(template.TemplateId, out var currentTemplateQuestions))
                {
                    continue;
                }

                var rendered = new HashSet<Guid>();
                var rootQuestions = currentTemplateQuestions
                    .Where(x => !childTemplateQuestionIds.Contains(x.TemplateQuestionId))
                    .OrderBy(x => x.Order)
                    .ToArray();

                var rootIndex = 1;

                foreach (var rootQuestion in rootQuestions)
                {
                    AddQuestionFlowLine(
                        templateKind,
                        rootQuestion,
                        number: rootIndex.ToString(),
                        depth: 0,
                        isRoot: true,
                        templateQuestionsById,
                        conditionsByParent,
                        optionsByQuestion,
                        result,
                        rendered,
                        new HashSet<Guid>());

                    rootIndex++;
                }

                foreach (var orphanQuestion in currentTemplateQuestions.Where(x => !rendered.Contains(x.TemplateQuestionId)))
                {
                    AddQuestionFlowLine(
                        templateKind,
                        orphanQuestion,
                        number: rootIndex.ToString(),
                        depth: 0,
                        isRoot: !childTemplateQuestionIds.Contains(orphanQuestion.TemplateQuestionId),
                        templateQuestionsById,
                        conditionsByParent,
                        optionsByQuestion,
                        result,
                        rendered,
                        new HashSet<Guid>());

                    rootIndex++;
                }
            }

            return result;
        }

        private static void AddQuestionFlowLine(
            ReportTemplateKind templateKind,
            TemplateQuestionFlatDto question,
            string number,
            int depth,
            bool isRoot,
            IReadOnlyDictionary<Guid, TemplateQuestionFlatDto> templateQuestionsById,
            IReadOnlyDictionary<Guid, ConditionFlatDto[]> conditionsByParent,
            IReadOnlyDictionary<Guid, QuestionOptionFlatDto[]> optionsByQuestion,
            List<BranchTemplatesPdfFlowLine> result,
            HashSet<Guid> rendered,
            HashSet<Guid> path)
        {
            if (!path.Add(question.TemplateQuestionId))
            {
                return;
            }

            rendered.Add(question.TemplateQuestionId);

            result.Add(new BranchTemplatesPdfFlowLine
            {
                TemplateId = question.TemplateId,
                TemplateKind = templateKind,
                Number = number,
                Depth = depth,
                LineKind = isRoot
                    ? BranchTemplatesPdfFlowLineKind.RootQuestion
                    : BranchTemplatesPdfFlowLineKind.ConditionalQuestion,
                TextEn = question.QuestionTextEn,
                TextAr = question.QuestionTextAr,
                QuestionType = question.QuestionType.ToString()
            });

            conditionsByParent.TryGetValue(
                question.TemplateQuestionId,
                out var parentConditions);
            parentConditions ??= Array.Empty<ConditionFlatDto>();

            var nextTriggerIndex = 1;

            if (question.QuestionType == QuestionType.SingleChoice &&
                optionsByQuestion.TryGetValue(question.QuestionId, out var options))
            {
                foreach (var option in options)
                {
                    var optionNumber = $"{number}.{nextTriggerIndex}";

                    result.Add(new BranchTemplatesPdfFlowLine
                    {
                        TemplateId = question.TemplateId,
                        TemplateKind = templateKind,
                        Number = optionNumber,
                        Depth = depth + 1,
                        LineKind = BranchTemplatesPdfFlowLineKind.Trigger,
                        TextEn = option.TextEn,
                        TextAr = option.TextAr,
                        Value = option.Value
                    });

                    var childIndex = 1;

                    foreach (var condition in parentConditions
                                 .Where(x =>
                                     x.TriggerType == QuestionConditionTriggerType.SingleChoiceOption &&
                                     x.SelectedQuestionOptionId == option.OptionId)
                                 .OrderBy(x => GetChildQuestionOrder(x, templateQuestionsById)))
                    {
                        if (templateQuestionsById.TryGetValue(
                                condition.ChildTemplateQuestionId,
                                out var childQuestion))
                        {
                            AddQuestionFlowLine(
                                templateKind,
                                childQuestion,
                                $"{optionNumber}.{childIndex}",
                                depth + 2,
                                isRoot: false,
                                templateQuestionsById,
                                conditionsByParent,
                                optionsByQuestion,
                                result,
                                rendered,
                                path);
                        }

                        childIndex++;
                    }

                    nextTriggerIndex++;
                }
            }

            foreach (var triggerGroup in parentConditions
                         .Where(x => x.TriggerType != QuestionConditionTriggerType.SingleChoiceOption)
                         .GroupBy(x => new { x.TriggerType, x.TriggerValue })
                         .OrderBy(x => x.Key.TriggerType)
                         .ThenBy(x => x.Key.TriggerValue ?? 0))
            {
                var triggerNumber = $"{number}.{nextTriggerIndex}";
                var triggerText = BuildValueTriggerText(
                    triggerGroup.Key.TriggerType,
                    triggerGroup.Key.TriggerValue);

                result.Add(new BranchTemplatesPdfFlowLine
                {
                    TemplateId = question.TemplateId,
                    TemplateKind = templateKind,
                    Number = triggerNumber,
                    Depth = depth + 1,
                    LineKind = BranchTemplatesPdfFlowLineKind.Trigger,
                    TextEn = triggerText.TextEn,
                    TextAr = triggerText.TextAr,
                    Value = triggerGroup.Key.TriggerValue
                });

                var childIndex = 1;

                foreach (var condition in triggerGroup.OrderBy(x => GetChildQuestionOrder(x, templateQuestionsById)))
                {
                    if (templateQuestionsById.TryGetValue(
                            condition.ChildTemplateQuestionId,
                            out var childQuestion))
                    {
                        AddQuestionFlowLine(
                            templateKind,
                            childQuestion,
                            $"{triggerNumber}.{childIndex}",
                            depth + 2,
                            isRoot: false,
                            templateQuestionsById,
                            conditionsByParent,
                            optionsByQuestion,
                            result,
                            rendered,
                            path);
                    }

                    childIndex++;
                }

                nextTriggerIndex++;
            }

            path.Remove(question.TemplateQuestionId);
        }

        private static IReadOnlyCollection<BranchTemplatesPdfOptionAnalytics> BuildOptionAnalytics(
            TemplateQuestionFlatDto templateQuestion,
            IReadOnlyCollection<QuestionOptionFlatDto> allOptions,
            IReadOnlyCollection<AnswerFlatDto> answers)
        {
            if (templateQuestion.QuestionType != QuestionType.SingleChoice)
            {
                return Array.Empty<BranchTemplatesPdfOptionAnalytics>();
            }

            var options = allOptions
                .Where(x => x.QuestionId == templateQuestion.QuestionId)
                .OrderBy(x => x.Order)
                .ToArray();

            var total = answers.Count(x => x.SelectedQuestionOptionId.HasValue);

            return options
                .Select(option =>
                {
                    var count = answers.Count(x => x.SelectedQuestionOptionId == option.OptionId);

                    return new BranchTemplatesPdfOptionAnalytics
                    {
                        LabelEn = option.TextEn,
                        LabelAr = option.TextAr,
                        Value = option.Value,
                        Count = count,
                        Percentage = total == 0
                            ? 0
                            : Math.Round(count * 100m / total, 2)
                    };
                })
                .ToArray();
        }

        private static decimal? CalculateAverageValue(
            QuestionType questionType,
            IReadOnlyCollection<AnswerFlatDto> answers,
            IReadOnlyCollection<QuestionOptionFlatDto> options)
        {
            if (answers.Count == 0)
            {
                return null;
            }

            if (questionType == QuestionType.StarRating)
            {
                var values = answers
                    .Where(x => x.StarRatingValue.HasValue)
                    .Select(x => (decimal)x.StarRatingValue!.Value)
                    .ToArray();

                return values.Length == 0 ? null : Math.Round(values.Average(), 2);
            }

            if (questionType == QuestionType.Smiles)
            {
                var values = answers
                    .Where(x => x.SmileValue.HasValue)
                    .Select(x => (decimal)x.SmileValue!.Value)
                    .ToArray();

                return values.Length == 0 ? null : Math.Round(values.Average(), 2);
            }

            if (questionType == QuestionType.SingleChoice)
            {
                var optionValuesById = options
                    .ToDictionary(x => x.OptionId, x => x.Value);

                var values = new List<decimal>();

                foreach (var answer in answers)
                {
                    if (!answer.SelectedQuestionOptionId.HasValue)
                    {
                        continue;
                    }

                    if (optionValuesById.TryGetValue(
                            answer.SelectedQuestionOptionId.Value,
                            out var optionValue))
                    {
                        values.Add(optionValue);
                    }
                }

                var arr = values.ToArray();

                return arr.Length == 0 ? null : Math.Round(arr.Average(), 2);
            }

            return null;
        }

        private static IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> BuildTemplateSummaries(
            IReadOnlyCollection<TemplateHeaderDto> normalTemplates,
            IReadOnlyCollection<TemplateHeaderDto> anonymousTemplates,
            IReadOnlyCollection<ResponseFlatDto> normalResponses,
            IReadOnlyCollection<ResponseFlatDto> anonymousResponses,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> normalQuestions,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> anonymousQuestions,
            IReadOnlyCollection<ScoredAnswerTokenDto> normalScoreTokens,
            IReadOnlyCollection<ScoredAnswerTokenDto> anonymousScoreTokens)
        {
            var summaries = new List<BranchTemplatesPdfTemplateSummary>();

            summaries.AddRange(BuildTemplateSummariesForKind(
                normalTemplates,
                normalResponses,
                normalQuestions,
                normalScoreTokens,
                ReportTemplateKind.Normal));

            summaries.AddRange(BuildTemplateSummariesForKind(
                anonymousTemplates,
                anonymousResponses,
                anonymousQuestions,
                anonymousScoreTokens,
                ReportTemplateKind.Anonymous));

            return summaries
                .OrderBy(x => x.TemplateKind)
                .ThenBy(x => x.NameEn)
                .ToArray();
        }

        private static IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> BuildTemplateSummariesForKind(
            IReadOnlyCollection<TemplateHeaderDto> templates,
            IReadOnlyCollection<ResponseFlatDto> responses,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> questions,
            IReadOnlyCollection<ScoredAnswerTokenDto> scoreTokens,
            ReportTemplateKind templateKind)
        {
            var responsesByTemplate = responses
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var questionsByTemplate = questions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var scoreTokensByTemplate = scoreTokens
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            return templates.Select(template =>
            {
                responsesByTemplate.TryGetValue(template.TemplateId, out var templateResponses);
                templateResponses ??= Array.Empty<ResponseFlatDto>();

                questionsByTemplate.TryGetValue(template.TemplateId, out var templateQuestions);
                templateQuestions ??= Array.Empty<BranchTemplatesPdfQuestionAnalytics>();

                scoreTokensByTemplate.TryGetValue(template.TemplateId, out var templateScoreTokens);
                templateScoreTokens ??= Array.Empty<ScoredAnswerTokenDto>();

                var avgValue = templateScoreTokens.Length == 0
                    ? null
                    : (decimal?)Math.Round(templateScoreTokens.Average(x => x.ScoreValue), 2);

                return new BranchTemplatesPdfTemplateSummary
                {
                    TemplateId = template.TemplateId,
                    TemplateKind = templateKind,
                    NameEn = template.NameEn,
                    NameAr = template.NameAr,
                    Status = template.Status,
                    ActiveFrom = template.ActiveFrom,
                    ExpireTo = template.ExpireTo,
                    TotalQuestions = templateQuestions.Length,
                    RootQuestions = templateQuestions.Count(x => x.IsRootQuestion),
                    ConditionalQuestions = templateQuestions.Count(x => !x.IsRootQuestion),
                    TotalResponses = templateResponses.Length,
                    TotalAnswers = templateQuestions.Sum(x => x.TotalAnswers),
                    TotalScoredAnswers = templateScoreTokens.Length,
                    AverageScoreValue = avgValue,
                    AverageScorePercentage = avgValue.HasValue
                        ? Math.Round(avgValue.Value * 100m / MaxScoreValue, 2)
                        : null
                };
            }).ToArray();
        }

        private static BranchTemplatesPdfExecutiveSummary BuildExecutiveSummary(
            IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> templates,
            IReadOnlyCollection<ResponseFlatDto> normalResponses,
            IReadOnlyCollection<ResponseFlatDto> anonymousResponses,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> questions,
            IReadOnlyCollection<ScoredAnswerTokenDto> scoreTokens,
            bool isArabic)
        {
            var allResponses = normalResponses.Concat(anonymousResponses).ToArray();

            var answeredTemplates = templates
                .Where(x => x.TotalResponses > 0)
                .ToArray();

            var highest = answeredTemplates
                .Where(x => x.AverageScorePercentage.HasValue)
                .OrderByDescending(x => x.AverageScorePercentage)
                .FirstOrDefault();

            var lowest = answeredTemplates
                .Where(x => x.AverageScorePercentage.HasValue)
                .OrderBy(x => x.AverageScorePercentage)
                .FirstOrDefault();

            var mostAnswered = templates
                .OrderByDescending(x => x.TotalResponses)
                .FirstOrDefault();

            var totalAnswers = questions.Sum(x => x.TotalAnswers);

            var avgValue = scoreTokens.Count == 0
                ? null
                : (decimal?)Math.Round(scoreTokens.Average(x => x.ScoreValue), 2);

            return new BranchTemplatesPdfExecutiveSummary
            {
                TotalNormalTemplates = templates.Count(x => x.TemplateKind == ReportTemplateKind.Normal),
                TotalAnonymousTemplates = templates.Count(x => x.TemplateKind == ReportTemplateKind.Anonymous),
                TotalResponses = allResponses.Length,
                TotalNormalResponses = normalResponses.Count,
                TotalAnonymousResponses = anonymousResponses.Count,
                TotalAnswers = totalAnswers,
                TotalScoredAnswers = scoreTokens.Count,
                TotalNonScoredAnswers = Math.Max(0, totalAnswers - scoreTokens.Count),
                AverageScoreValue = avgValue,
                AverageScorePercentage = avgValue.HasValue
                    ? Math.Round(avgValue.Value * 100m / MaxScoreValue, 2)
                    : null,
                HighestRatedTemplateName = highest?.DisplayName(isArabic) ?? "-",
                LowestRatedTemplateName = lowest?.DisplayName(isArabic) ?? "-",
                MostAnsweredTemplateName = mostAnswered?.DisplayName(isArabic) ?? "-",
                TemplatesWithoutResponses = templates.Count(x => x.TotalResponses == 0)
            };
        }

        private static IReadOnlyCollection<BranchTemplatesPdfQuestionRankItem> BuildQuestionRanks(
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> questions,
            int count,
            bool worst)
        {
            var scoredQuestions = questions
                .Where(x => x.IsScoreIncluded && x.ScoreAverageValue.HasValue)
                .ToArray();

            var ordered = worst
                ? scoredQuestions
                    .OrderBy(x => x.ScoreAverageValue!.Value)
                    .ThenByDescending(x => x.TotalAnswers)
                    .ThenBy(x => x.QuestionTextEn)
                : scoredQuestions
                    .OrderByDescending(x => x.ScoreAverageValue!.Value)
                    .ThenByDescending(x => x.TotalAnswers)
                    .ThenBy(x => x.QuestionTextEn);

            return ordered
                .Take(count)
                .Select((question, index) => new BranchTemplatesPdfQuestionRankItem
                {
                    Rank = index + 1,
                    TemplateId = question.TemplateId,
                    TemplateKind = question.TemplateKind,
                    TemplateNameEn = question.TemplateNameEn,
                    TemplateNameAr = question.TemplateNameAr,
                    TemplateQuestionId = question.TemplateQuestionId,
                    QuestionTextEn = question.QuestionTextEn,
                    QuestionTextAr = question.QuestionTextAr,
                    IsRootQuestion = question.IsRootQuestion,
                    QuestionType = question.QuestionType,
                    TotalAnswers = question.TotalAnswers,
                    AverageScoreValue = question.ScoreAverageValue!.Value,
                    SatisfactionPercentage = Math.Round(
                        question.ScoreAverageValue!.Value * 100m / MaxScoreValue,
                        2)
                })
                .ToArray();
        }

        private static IReadOnlyCollection<BranchTemplatesPdfTemplateDetail> BuildTemplateDetails(
            IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> templates,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> questions,
            IReadOnlyCollection<BranchTemplatesPdfFlowLine> flowLines)
        {
            var questionsByTemplate = questions
                .GroupBy(x => new
                {
                    x.TemplateKind,
                    x.TemplateId
                })
                .ToDictionary(
                    x => (x.Key.TemplateKind, x.Key.TemplateId),
                    x => x.OrderByDescending(q => q.IsRootQuestion)
                        .ThenBy(q => q.QuestionTextEn)
                        .ToArray());

            var flowLinesByTemplate = flowLines
                .GroupBy(x => new
                {
                    x.TemplateKind,
                    x.TemplateId
                })
                .ToDictionary(
                    x => (x.Key.TemplateKind, x.Key.TemplateId),
                    x => x.ToArray());

            return templates
                .Select(template =>
                {
                    questionsByTemplate.TryGetValue(
                        (template.TemplateKind, template.TemplateId),
                        out var templateQuestions);
                    templateQuestions ??= Array.Empty<BranchTemplatesPdfQuestionAnalytics>();

                    flowLinesByTemplate.TryGetValue(
                        (template.TemplateKind, template.TemplateId),
                        out var templateFlowLines);
                    templateFlowLines ??= Array.Empty<BranchTemplatesPdfFlowLine>();

                    return new BranchTemplatesPdfTemplateDetail
                    {
                        Summary = template,
                        Questions = templateQuestions,
                        FlowLines = templateFlowLines
                    };
                })
                .ToArray();
        }

        private static Error? ValidateSelectedTemplateResolution(
            BranchTemplatesPdfReportRequest request,
            IReadOnlyCollection<TemplateHeaderDto> normalTemplates,
            IReadOnlyCollection<TemplateHeaderDto> anonymousTemplates)
        {
            if (!request.TemplateId.HasValue)
            {
                return null;
            }

            var normalMatch = normalTemplates.Any(x => x.TemplateId == request.TemplateId.Value);
            var anonymousMatch = anonymousTemplates.Any(x => x.TemplateId == request.TemplateId.Value);

            if (!normalMatch && !anonymousMatch)
            {
                return new Error(
                    Code: "Reports.TemplatesPdf.TemplateNotFound",
                    Message: ErrorMessage.GetBranchTemplatesPdfReport_Template_NotFound,
                    Type: ErrorType.NotFound);
            }

            if (!request.TemplateKind.HasValue && normalMatch && anonymousMatch)
            {
                return new Error(
                    Code: "Reports.TemplatesPdf.TemplateKindRequiredForAmbiguousTemplate",
                    Message: ErrorMessage.GetBranchTemplatesPdfReport_TemplateKind_Required_ForAmbiguousTemplate,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static ReportTemplateKind? ResolveSelectedTemplateKind(
            BranchTemplatesPdfReportRequest request,
            IReadOnlyCollection<TemplateHeaderDto> normalTemplates,
            IReadOnlyCollection<TemplateHeaderDto> anonymousTemplates)
        {
            if (request.TemplateKind.HasValue)
            {
                return request.TemplateKind.Value;
            }

            if (!request.TemplateId.HasValue)
            {
                return null;
            }

            if (normalTemplates.Any(x => x.TemplateId == request.TemplateId.Value))
            {
                return ReportTemplateKind.Normal;
            }

            if (anonymousTemplates.Any(x => x.TemplateId == request.TemplateId.Value))
            {
                return ReportTemplateKind.Anonymous;
            }

            return null;
        }

        private static string ResolveSelectedTemplateName(
            BranchTemplatesPdfReportRequest request,
            IReadOnlyCollection<TemplateHeaderDto> normalTemplates,
            IReadOnlyCollection<TemplateHeaderDto> anonymousTemplates,
            bool isArabic)
        {
            if (!request.TemplateId.HasValue)
            {
                return string.Empty;
            }

            var normal = normalTemplates.FirstOrDefault(x => x.TemplateId == request.TemplateId.Value);

            if (normal is not null)
            {
                return normal.DisplayName(isArabic);
            }

            var anonymous = anonymousTemplates.FirstOrDefault(x => x.TemplateId == request.TemplateId.Value);

            return anonymous?.DisplayName(isArabic) ?? string.Empty;
        }

        private static IReadOnlyCollection<AnswerFlatDto> AttachTemplateIds(
            IReadOnlyCollection<AnswerFlatDto> answers,
            IReadOnlyCollection<ResponseFlatDto> responses)
        {
            var templateIdByResponseId = responses
                .ToDictionary(x => x.ResponseId, x => x.TemplateId);

            return answers
                .Select(answer =>
                    templateIdByResponseId.TryGetValue(answer.ResponseId, out var templateId)
                        ? answer with { TemplateId = templateId }
                        : answer)
                .Where(answer => answer.TemplateId != Guid.Empty)
                .ToArray();
        }

        private static ParentTriggerText BuildParentTriggerText(
            ConditionFlatDto condition,
            IReadOnlyDictionary<Guid, QuestionOptionFlatDto> questionOptionsById)
        {
            if (condition.TriggerType == QuestionConditionTriggerType.SingleChoiceOption &&
                condition.SelectedQuestionOptionId.HasValue &&
                questionOptionsById.TryGetValue(condition.SelectedQuestionOptionId.Value, out var option))
            {
                var arabicLabel = string.IsNullOrWhiteSpace(option.TextAr)
                    ? option.TextEn
                    : option.TextAr!;

                return new ParentTriggerText(
                    TextEn: $"Option: {option.TextEn} (Value: {option.Value})",
                    TextAr: $"الاختيار: {arabicLabel} (القيمة: {option.Value})");
            }

            return BuildValueTriggerText(
                condition.TriggerType,
                condition.TriggerValue);
        }

        private static ParentTriggerText BuildValueTriggerText(
            QuestionConditionTriggerType triggerType,
            int? triggerValue)
        {
            var value = triggerValue?.ToString() ?? "-";

            return triggerType switch
            {
                QuestionConditionTriggerType.StarRatingValue => new ParentTriggerText(
                    TextEn: $"Star rating value: {value}",
                    TextAr: $"قيمة تقييم النجوم: {value}"),

                QuestionConditionTriggerType.SmileValue => new ParentTriggerText(
                    TextEn: $"Smile value: {value}",
                    TextAr: $"قيمة الوجه التعبيري: {value}"),

                _ => new ParentTriggerText(
                    TextEn: $"Value: {value}",
                    TextAr: $"القيمة: {value}")
            };
        }

        private static int GetChildQuestionOrder(
            ConditionFlatDto condition,
            IReadOnlyDictionary<Guid, TemplateQuestionFlatDto> templateQuestionsById)
            => templateQuestionsById.TryGetValue(condition.ChildTemplateQuestionId, out var child)
                ? child.Order
                : int.MaxValue;

        private static bool IsScoredQuestionType(QuestionType questionType)
            => questionType is QuestionType.SingleChoice
                or QuestionType.StarRating
                or QuestionType.Smiles;

        private sealed record QuestionAnalyticsBuildResult(
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> Questions,
            IReadOnlyCollection<ScoredAnswerTokenDto> ScoreTokens,
            IReadOnlyCollection<BranchTemplatesPdfFlowLine> FlowLines)
        {
            public static QuestionAnalyticsBuildResult Empty()
                => new(
                    Array.Empty<BranchTemplatesPdfQuestionAnalytics>(),
                    Array.Empty<ScoredAnswerTokenDto>(),
                    Array.Empty<BranchTemplatesPdfFlowLine>());
        }

        private sealed record TemplateHeaderDto
        {
            public Guid TemplateId { get; init; }

            public ReportTemplateKind TemplateKind { get; init; }

            public string NameEn { get; init; } = string.Empty;

            public string? NameAr { get; init; }

            public string DisplayName(bool isArabic)
                => isArabic && !string.IsNullOrWhiteSpace(NameAr) ? NameAr! : NameEn;

            public string Status { get; init; } = string.Empty;

            public DateTime? ActiveFrom { get; init; }

            public DateTime? ExpireTo { get; init; }
        }

        private sealed record ResponseFlatDto
        {
            public Guid ResponseId { get; init; }

            public Guid TemplateId { get; init; }

            public ReportTemplateKind TemplateKind { get; init; }

            public DateTime SubmittedOnUtc { get; init; }
        }

        private sealed record TemplateQuestionFlatDto
        {
            public Guid TemplateQuestionId { get; init; }

            public Guid TemplateId { get; init; }

            public Guid QuestionId { get; init; }

            public int Order { get; init; }

            public string QuestionTextEn { get; init; } = string.Empty;

            public string? QuestionTextAr { get; init; }

            public QuestionType QuestionType { get; init; }
        }

        private sealed record ConditionFlatDto
        {
            public Guid TemplateId { get; init; }

            public Guid ParentTemplateQuestionId { get; init; }

            public Guid ChildTemplateQuestionId { get; init; }

            public QuestionConditionTriggerType TriggerType { get; init; }

            public Guid? SelectedQuestionOptionId { get; init; }

            public int? TriggerValue { get; init; }
        }

        private sealed record AnswerFlatDto
        {
            public Guid ResponseId { get; init; }

            public Guid TemplateId { get; init; }

            public Guid QuestionId { get; init; }

            public QuestionType QuestionType { get; init; }

            public Guid? SelectedQuestionOptionId { get; init; }

            public int? StarRatingValue { get; init; }

            public int? SmileValue { get; init; }

            public bool HasTextAnswer { get; init; }

            public bool HasVoiceAnswer { get; init; }
        }

        private sealed record QuestionOptionFlatDto
        {
            public Guid OptionId { get; init; }

            public Guid QuestionId { get; init; }

            public string TextEn { get; init; } = string.Empty;

            public string? TextAr { get; init; }

            public int Value { get; init; }

            public int Order { get; init; }
        }

        private sealed record ScoredAnswerTokenDto
        {
            public Guid ResponseId { get; init; }

            public Guid TemplateId { get; init; }

            public Guid TemplateQuestionId { get; init; }

            public Guid QuestionId { get; init; }

            public decimal ScoreValue { get; init; }
        }

        private sealed record ParentTriggerText(
            string TextEn,
            string TextAr);
    }
}
