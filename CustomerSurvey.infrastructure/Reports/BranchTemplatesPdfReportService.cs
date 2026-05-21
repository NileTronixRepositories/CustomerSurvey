using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Reports
{
    internal sealed class BranchTemplatesPdfReportService : IBranchTemplatesPdfReportService
    {
        private const string ViewPath = "/Views/Reports/BranchTemplatesPdfReport.cshtml";
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
        private readonly IWriteReadRepository<AnonymousTemplateCustomInput> _anonymousTemplateCustomInputRepository;
        private readonly IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> _anonymousSurveyResponseCustomInputValueRepository;
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
      IWriteReadRepository<AnonymousTemplateCustomInput> anonymousTemplateCustomInputRepository,
      IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> anonymousSurveyResponseCustomInputValueRepository,
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
            _anonymousTemplateCustomInputRepository = anonymousTemplateCustomInputRepository ?? throw new ArgumentNullException(nameof(anonymousTemplateCustomInputRepository));
            _anonymousSurveyResponseCustomInputValueRepository = anonymousSurveyResponseCustomInputValueRepository ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseCustomInputValueRepository));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        }

        public async Task<BranchTemplatesPdfReportFile> GenerateAsync(
     BranchTemplatesPdfReportRequest request,
     CancellationToken cancellationToken)
        {
            var isArabic = string.Equals(
                request.Language,
                "ar",
                StringComparison.OrdinalIgnoreCase);

            var model = await BuildReportModelAsync(
                request,
                cancellationToken);

            var pdfBytes = await _pdfService.GeneratePdfAsync(
                viewName: "BranchTemplatesPdfReport",
                model: model,
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
                : $"customer-survey-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return new BranchTemplatesPdfReportFile
            {
                FileName = fileName,
                ContentType = "application/pdf",
                Content = pdfBytes
            };
        }

        private async Task<BranchTemplatesPdfReportModel> BuildReportModelAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
        {
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

            var customInputs = await LoadAnonymousCustomInputsSummaryAsync(
                anonymousTemplates,
                anonymousResponses,
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

            var allScoreTokens = normalQuestionBuild.ScoreTokens
                .Concat(anonymousQuestionBuild.ScoreTokens)
                .ToArray();

            var executiveSummary = BuildExecutiveSummary(
                templates,
                normalResponses,
                anonymousResponses,
                allQuestions,
                allScoreTokens);

            return new BranchTemplatesPdfReportModel
            {
                Language = request.Language,
                BranchName = string.Equals(request.Language, "ar", StringComparison.OrdinalIgnoreCase)
                             && !string.IsNullOrWhiteSpace(branch.NameAr)
                    ? branch.NameAr!
                    : branch.NameEn,
                GeneratedBy = request.GeneratedByName,
                GeneratedAtUtc = DateTime.UtcNow,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                SelectedTemplateId = request.TemplateId,
                SelectedTemplateKind = request.TemplateKind,
                SelectedTemplateName = ResolveSelectedTemplateName(
                    request,
                    normalTemplates,
                    anonymousTemplates),
                ScoreCalculationMode = request.ScoreCalculationMode,
                ExecutiveSummary = executiveSummary,
                Templates = templates,
                Questions = allQuestions,
                CustomInputs = customInputs
            };
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
                : await _surveyAnswerRepository.Query()
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
                    .ToArrayAsync(cancellationToken);

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
                : await _anonymousSurveyAnswerRepository.Query()
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
                    .ToArrayAsync(cancellationToken);

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

            var childTemplateQuestionIds = conditions
                .Select(x => x.ChildTemplateQuestionId)
                .Distinct()
                .ToHashSet();

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
                .GroupBy(x => x.QuestionId)
                .ToDictionary(x => x.Key, x => x.ToArray());

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

                answersByQuestion.TryGetValue(templateQuestion.QuestionId, out var questionAnswers);
                questionAnswers ??= Array.Empty<AnswerFlatDto>();

                scoreTokensByTemplateQuestion.TryGetValue(templateQuestion.TemplateQuestionId, out var questionScoreTokens);
                questionScoreTokens ??= Array.Empty<ScoredAnswerTokenDto>();

                var totalResponses = templateResponses.Length;
                var totalAnswers = questionAnswers.Length;
                var skipped = Math.Max(0, totalResponses - totalAnswers);

                var isRoot = !childTemplateQuestionIds.Contains(templateQuestion.TemplateQuestionId);

                var isScoreIncluded =
                    scoreCalculationMode == ScoreCalculationMode.RootQuestions
                        ? isRoot && IsScoredQuestionType(templateQuestion.QuestionType)
                        : questionScoreTokens.Length > 0;

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
                    TotalAnswers = totalAnswers,
                    SkippedCount = skipped,
                    AnswerRatePercentage = totalResponses == 0
                        ? 0
                        : Math.Round(totalAnswers * 100m / totalResponses, 2),
                    AverageValue = CalculateAverageValue(
                        templateQuestion.QuestionType,
                        questionAnswers,
                        questionOptions),
                    IsScoreIncluded = isScoreIncluded,
                    ScoreIncludedAnswersCount = questionScoreTokens.Length,
                    Options = BuildOptionAnalytics(
                        templateQuestion,
                        questionOptions,
                        questionAnswers)
                });
            }

            return new QuestionAnalyticsBuildResult(
                result,
                scoreTokens);
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
                    x => x.ToArray());

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
                var values =
                    from answer in answers
                    where answer.SelectedQuestionOptionId.HasValue
                    join option in options
                        on answer.SelectedQuestionOptionId.Value equals option.OptionId
                    select (decimal)option.Value;

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

        private async Task<IReadOnlyCollection<BranchTemplatesPdfCustomInputSummary>> LoadAnonymousCustomInputsSummaryAsync(
            IReadOnlyCollection<TemplateHeaderDto> anonymousTemplates,
            IReadOnlyCollection<ResponseFlatDto> anonymousResponses,
            CancellationToken cancellationToken)
        {
            var templateIds = anonymousTemplates.Select(x => x.TemplateId).ToArray();

            if (templateIds.Length == 0)
            {
                return Array.Empty<BranchTemplatesPdfCustomInputSummary>();
            }

            var customInputs = await _anonymousTemplateCustomInputRepository.Query()
                .Where(x => templateIds.Contains(x.AnonymousTemplateId))
                .Select(x => new
                {
                    x.Id,
                    x.AnonymousTemplateId,
                    x.Name,
                    x.LabelEn,
                    x.LabelAr,
                    Type = x.Type.ToString(),
                    x.IsRequired
                })
                .ToArrayAsync(cancellationToken);

            var responseIds = anonymousResponses.Select(x => x.ResponseId).ToArray();

            var values = responseIds.Length == 0
                ? []
                : await _anonymousSurveyResponseCustomInputValueRepository.Query()
                    .Where(x => responseIds.Contains(x.AnonymousSurveyResponseId))
                    .Select(x => new
                    {
                        x.AnonymousTemplateCustomInputId,
                        HasValue =
                            !string.IsNullOrWhiteSpace(x.StringValue) ||
                            x.IntegerValue.HasValue
                    })
                    .ToArrayAsync(cancellationToken);

            var responsesCountByTemplate = anonymousResponses
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.Count());

            var templatesById = anonymousTemplates.ToDictionary(x => x.TemplateId);

            return customInputs
                .Select(input =>
                {
                    var templateResponsesCount = responsesCountByTemplate.TryGetValue(
                        input.AnonymousTemplateId,
                        out var count)
                            ? count
                            : 0;

                    var filled = values.Count(x =>
                        x.AnonymousTemplateCustomInputId == input.Id &&
                        x.HasValue);

                    var empty = Math.Max(0, templateResponsesCount - filled);

                    var template = templatesById[input.AnonymousTemplateId];

                    return new BranchTemplatesPdfCustomInputSummary
                    {
                        AnonymousTemplateId = input.AnonymousTemplateId,
                        TemplateNameEn = template.NameEn,
                        TemplateNameAr = template.NameAr,
                        InputName = input.Name,
                        LabelEn = input.LabelEn,
                        LabelAr = input.LabelAr,
                        Type = input.Type,
                        IsRequired = input.IsRequired,
                        FilledCount = filled,
                        EmptyCount = empty,
                        CompletionRatePercentage = templateResponsesCount == 0
                            ? 0
                            : Math.Round(filled * 100m / templateResponsesCount, 2)
                    };
                })
                .OrderBy(x => x.TemplateNameEn)
                .ThenBy(x => x.InputName)
                .ToArray();
        }

        private static BranchTemplatesPdfExecutiveSummary BuildExecutiveSummary(
            IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> templates,
            IReadOnlyCollection<ResponseFlatDto> normalResponses,
            IReadOnlyCollection<ResponseFlatDto> anonymousResponses,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> questions,
            IReadOnlyCollection<ScoredAnswerTokenDto> scoreTokens)
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
                HighestRatedTemplateName = highest?.NameEn ?? "-",
                LowestRatedTemplateName = lowest?.NameEn ?? "-",
                MostAnsweredTemplateName = mostAnswered?.NameEn ?? "-",
                TemplatesWithoutResponses = templates.Count(x => x.TotalResponses == 0)
            };
        }

        private static string ResolveSelectedTemplateName(
            BranchTemplatesPdfReportRequest request,
            IReadOnlyCollection<TemplateHeaderDto> normalTemplates,
            IReadOnlyCollection<TemplateHeaderDto> anonymousTemplates)
        {
            if (!request.TemplateId.HasValue)
            {
                return string.Empty;
            }

            var normal = normalTemplates.FirstOrDefault(x => x.TemplateId == request.TemplateId.Value);

            if (normal is not null)
            {
                return normal.NameEn;
            }

            var anonymous = anonymousTemplates.FirstOrDefault(x => x.TemplateId == request.TemplateId.Value);

            return anonymous?.NameEn ?? string.Empty;
        }

        private static bool IsScoredQuestionType(QuestionType questionType)
            => questionType is QuestionType.SingleChoice
                or QuestionType.StarRating
                or QuestionType.Smiles;

        private sealed record QuestionAnalyticsBuildResult(
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> Questions,
            IReadOnlyCollection<ScoredAnswerTokenDto> ScoreTokens)
        {
            public static QuestionAnalyticsBuildResult Empty()
                => new(
                    Array.Empty<BranchTemplatesPdfQuestionAnalytics>(),
                    Array.Empty<ScoredAnswerTokenDto>());
        }

        private sealed record TemplateHeaderDto
        {
            public Guid TemplateId { get; init; }

            public ReportTemplateKind TemplateKind { get; init; }

            public string NameEn { get; init; } = string.Empty;

            public string? NameAr { get; init; }

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
    }
}