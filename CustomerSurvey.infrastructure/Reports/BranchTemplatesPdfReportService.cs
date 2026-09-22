using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Reports
{
    internal sealed class BranchTemplatesPdfReportService : IBranchTemplatesPdfReportService
    {
        private const decimal MaxScoreValue = 5m;
        private const string SurveyVoiceAnswersBasePath = "Media/SurveyVoiceAnswers";
        private const string SurveyAnswerImagesBasePath = "Media/SurveyAnswerImages";

        private readonly IWriteReadRepository<Branch> _branchRepository;
        private readonly IWriteReadRepository<Template> _templateRepository;
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateRepository;
        private readonly IWriteReadRepository<SurveyResponse> _surveyResponseRepository;
        private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerRepository;
        private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _surveyResponseCustomInputValueRepository;
        private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseRepository;
        private readonly IWriteReadRepository<AnonymousSurveyAnswer> _anonymousSurveyAnswerRepository;
        private readonly IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> _anonymousSurveyResponseCustomInputValueRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionRepository;
        private readonly IWriteReadRepository<TemplateCustomInput> _templateCustomInputRepository;
        private readonly IWriteReadRepository<AnonymousTemplateCustomInput> _anonymousTemplateCustomInputRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _templateQuestionConditionRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _anonymousTemplateQuestionConditionRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionRepository;
        private readonly ISurveyReportScoringService _surveyReportScoringService;
        private readonly IPdfService _pdfService;

        public BranchTemplatesPdfReportService(
            IWriteReadRepository<Branch> branchRepository,
            IWriteReadRepository<Template> templateRepository,
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateRepository,
            IWriteReadRepository<SurveyResponse> surveyResponseRepository,
            IWriteReadRepository<SurveyAnswer> surveyAnswerRepository,
            IWriteReadRepository<SurveyResponseCustomInputValue> surveyResponseCustomInputValueRepository,
            IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseRepository,
            IWriteReadRepository<AnonymousSurveyAnswer> anonymousSurveyAnswerRepository,
            IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> anonymousSurveyResponseCustomInputValueRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionRepository,
            IWriteReadRepository<TemplateCustomInput> templateCustomInputRepository,
            IWriteReadRepository<AnonymousTemplateCustomInput> anonymousTemplateCustomInputRepository,
            IWriteReadRepository<TemplateQuestionCondition> templateQuestionConditionRepository,
            IWriteReadRepository<AnonymousTemplateQuestionCondition> anonymousTemplateQuestionConditionRepository,
            IWriteReadRepository<QuestionOption> questionOptionRepository,
            ISurveyReportScoringService surveyReportScoringService,
            IPdfService pdfService)
        {
            _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _anonymousTemplateRepository = anonymousTemplateRepository ?? throw new ArgumentNullException(nameof(anonymousTemplateRepository));
            _surveyResponseRepository = surveyResponseRepository ?? throw new ArgumentNullException(nameof(surveyResponseRepository));
            _surveyAnswerRepository = surveyAnswerRepository ?? throw new ArgumentNullException(nameof(surveyAnswerRepository));
            _surveyResponseCustomInputValueRepository = surveyResponseCustomInputValueRepository ?? throw new ArgumentNullException(nameof(surveyResponseCustomInputValueRepository));
            _anonymousSurveyResponseRepository = anonymousSurveyResponseRepository ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseRepository));
            _anonymousSurveyAnswerRepository = anonymousSurveyAnswerRepository ?? throw new ArgumentNullException(nameof(anonymousSurveyAnswerRepository));
            _anonymousSurveyResponseCustomInputValueRepository = anonymousSurveyResponseCustomInputValueRepository ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseCustomInputValueRepository));
            _templateQuestionRepository = templateQuestionRepository ?? throw new ArgumentNullException(nameof(templateQuestionRepository));
            _anonymousTemplateQuestionRepository = anonymousTemplateQuestionRepository ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionRepository));
            _templateCustomInputRepository = templateCustomInputRepository ?? throw new ArgumentNullException(nameof(templateCustomInputRepository));
            _anonymousTemplateCustomInputRepository = anonymousTemplateCustomInputRepository ?? throw new ArgumentNullException(nameof(anonymousTemplateCustomInputRepository));
            _templateQuestionConditionRepository = templateQuestionConditionRepository ?? throw new ArgumentNullException(nameof(templateQuestionConditionRepository));
            _anonymousTemplateQuestionConditionRepository = anonymousTemplateQuestionConditionRepository ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionConditionRepository));
            _questionOptionRepository = questionOptionRepository ?? throw new ArgumentNullException(nameof(questionOptionRepository));
            _surveyReportScoringService = surveyReportScoringService ?? throw new ArgumentNullException(nameof(surveyReportScoringService));
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
                PreparePdfRequest(request),
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

        internal static BranchTemplatesPdfReportRequest PreparePdfRequest(
            BranchTemplatesPdfReportRequest request)
            => request with { IncludeResponseDetails = true };

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
                .AsNoTracking()
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
                request.IncludeResponseDetails,
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
                request.IncludeResponseDetails,
                cancellationToken);

            var anonymousQuestionBuild = await LoadAnonymousQuestionAnalyticsAsync(
                anonymousTemplates,
                anonymousResponses,
                request.ScoreCalculationMode,
                request.IncludeResponseDetails,
                cancellationToken);

            var templates = BuildTemplateSummaries(
                normalTemplates,
                anonymousTemplates,
                normalResponses,
                anonymousResponses,
                normalQuestionBuild.Questions,
                anonymousQuestionBuild.Questions,
                normalQuestionBuild.ScoreTokens,
                anonymousQuestionBuild.ScoreTokens,
                normalQuestionBuild.ResponseScores,
                anonymousQuestionBuild.ResponseScores);

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

            var allResponseScores = normalQuestionBuild.ResponseScores
                .Concat(anonymousQuestionBuild.ResponseScores)
                .ToArray();

            IReadOnlyCollection<BranchTemplatesReportCustomInputDefinition> customInputDefinitions =
                Array.Empty<BranchTemplatesReportCustomInputDefinition>();
            IReadOnlyCollection<BranchTemplatesReportResponse> reportResponses =
                Array.Empty<BranchTemplatesReportResponse>();

            if (request.IncludeResponseDetails)
            {
                var normalCustomInputDefinitions = await LoadNormalCustomInputDefinitionsAsync(
                    normalTemplateIds,
                    cancellationToken);
                var anonymousCustomInputDefinitions = await LoadAnonymousCustomInputDefinitionsAsync(
                    anonymousTemplateIds,
                    cancellationToken);

                customInputDefinitions = normalCustomInputDefinitions
                    .Concat(anonymousCustomInputDefinitions)
                    .OrderBy(x => x.TemplateKind)
                    .ThenBy(x => x.Order)
                    .ThenBy(x => x.Name)
                    .ToArray();

                var normalCustomInputValues = await LoadNormalCustomInputValuesAsync(
                    normalResponses,
                    normalCustomInputDefinitions,
                    cancellationToken);
                var anonymousCustomInputValues = await LoadAnonymousCustomInputValuesAsync(
                    anonymousResponses,
                    anonymousCustomInputDefinitions,
                    cancellationToken);

                reportResponses = BuildReportResponses(
                    normalTemplates,
                    anonymousTemplates,
                    normalResponses,
                    anonymousResponses,
                    allResponseScores,
                    normalQuestionBuild.Answers.Concat(anonymousQuestionBuild.Answers).ToArray(),
                    normalCustomInputValues.Concat(anonymousCustomInputValues).ToArray());
            }

            var executiveSummary = BuildExecutiveSummary(
                templates,
                normalResponses,
                anonymousResponses,
                allQuestions,
                allScoreTokens,
                allResponseScores,
                isArabic);

            var detailedAnswers = normalQuestionBuild.Answers
                .Concat(anonymousQuestionBuild.Answers)
                .ToArray();

            var graphics = BranchTemplatesReportGraphicsBuilder.Build(
                totalResponses: normalResponses.Count + anonymousResponses.Count,
                responseScores: allResponseScores,
                questions: allQuestions,
                totalAnswers: request.IncludeResponseDetails
                    ? detailedAnswers.Length
                    : executiveSummary.TotalAnswers,
                includedAnswers: request.IncludeResponseDetails
                    ? detailedAnswers.Count(x => x.IncludedInScore)
                    : allScoreTokens.Length,
                overallSatisfactionPercentage: executiveSummary.AverageScorePercentage,
                averageScoreValue: executiveSummary.AverageScoreValue);

            var templateDetails = BuildTemplateDetails(
                templates,
                allQuestions,
                allFlowLines);

            var worstQuestionsMaxScorePercentage =
                BranchTemplatesReportQuestionRankThresholds.ResolveWorstQuestionsMaxScorePercentage(
                    request.WorstQuestionsMaxScorePercentage);
            var bestQuestionsMinScorePercentage =
                BranchTemplatesReportQuestionRankThresholds.ResolveBestQuestionsMinScorePercentage(
                    request.BestQuestionsMinScorePercentage);

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
                WorstQuestionsMaxScorePercentage = worstQuestionsMaxScorePercentage,
                BestQuestionsMinScorePercentage = bestQuestionsMinScorePercentage,
                ExecutiveSummary = executiveSummary,
                Templates = templates,
                Questions = allQuestions,
                WorstQuestions = BranchTemplatesQuestionRankBuilder.Build(
                    allQuestions,
                    request.TopWorstQuestionsCount,
                    worst: true,
                    worstQuestionsMaxScorePercentage,
                    bestQuestionsMinScorePercentage),
                BestQuestions = BranchTemplatesQuestionRankBuilder.Build(
                    allQuestions,
                    request.TopWorstQuestionsCount,
                    worst: false,
                    worstQuestionsMaxScorePercentage,
                    bestQuestionsMinScorePercentage),
                TemplateDetails = templateDetails,
                CustomInputDefinitions = customInputDefinitions,
                Responses = reportResponses,
                Graphics = graphics
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
                .AsNoTracking()
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
                .AsNoTracking()
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
            bool includeResponseDetails,
            CancellationToken cancellationToken)
        {
            if (templateIds.Count == 0)
            {
                return Array.Empty<ResponseFlatDto>();
            }

            var query = _surveyResponseRepository.Query()
                .AsNoTracking()
                .Where(x =>
                    templateIds.Contains(x.TemplateId) &&
                    x.SubmittedOnUtc >= fromUtc &&
                    x.SubmittedOnUtc < toExclusiveUtc);

            if (!includeResponseDetails)
            {
                return await query
                    .Select(x => new ResponseFlatDto
                    {
                        ResponseId = x.Id,
                        TemplateId = x.TemplateId,
                        TemplateKind = ReportTemplateKind.Normal,
                        SubmittedOnUtc = x.SubmittedOnUtc
                    })
                    .ToArrayAsync(cancellationToken);
            }

            return await query
                .Select(x => new ResponseFlatDto
                {
                    ResponseId = x.Id,
                    TemplateId = x.TemplateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    SubmittedOnUtc = x.SubmittedOnUtc,
                    OperatorId = x.OperatorId,
                    OperatorNameEn = x.Operator.ApplicationUser.NameEn,
                    OperatorNameAr = x.Operator.ApplicationUser.NameAr
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
                .AsNoTracking()
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
            bool includeResponseDetails,
            CancellationToken cancellationToken)
        {
            var templateIds = templates.Select(x => x.TemplateId).ToArray();

            if (templateIds.Length == 0)
            {
                return QuestionAnalyticsBuildResult.Empty();
            }

            var templateQuestions = await _templateQuestionRepository.Query()
                .AsNoTracking()
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
                .AsNoTracking()
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
            IReadOnlyCollection<AnswerFlatDto> answers;

            if (responseIds.Length == 0)
            {
                answers = Array.Empty<AnswerFlatDto>();
            }
            else
            {
                var answerQuery = _surveyAnswerRepository.Query()
                    .AsNoTracking()
                    .Where(x => responseIds.Contains(x.SurveyResponseId));

                var loadedAnswers = includeResponseDetails
                    ? await answerQuery
                        .Select(x => new AnswerFlatDto
                        {
                            ResponseId = x.SurveyResponseId,
                            QuestionId = x.QuestionId,
                            QuestionType = x.QuestionType,
                            SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                            StarRatingValue = x.StarRatingValue,
                            SmileValue = x.SmileValue,
                            HasTextAnswer = !string.IsNullOrWhiteSpace(x.TextAnswer),
                            HasVoiceAnswer = !string.IsNullOrWhiteSpace(x.VoiceFileName),
                            QuestionTextEn = x.Question.TextEn,
                            QuestionTextAr = x.Question.TextAr,
                            TextAnswer = x.TextAnswer,
                            VoiceFileName = x.VoiceFileName,
                            ImageFileName = x.ImageFileName
                        })
                        .ToArrayAsync(cancellationToken)
                    : await answerQuery
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

                answers = AttachTemplateIds(loadedAnswers, responses);
            }

            return await BuildQuestionAnalyticsAsync(
                templates,
                templateQuestions,
                conditions,
                responses,
                answers,
                ReportTemplateKind.Normal,
                scoreCalculationMode,
                includeResponseDetails,
                cancellationToken);
        }

        private async Task<QuestionAnalyticsBuildResult> LoadAnonymousQuestionAnalyticsAsync(
            IReadOnlyCollection<TemplateHeaderDto> templates,
            IReadOnlyCollection<ResponseFlatDto> responses,
            ScoreCalculationMode scoreCalculationMode,
            bool includeResponseDetails,
            CancellationToken cancellationToken)
        {
            var templateIds = templates.Select(x => x.TemplateId).ToArray();

            if (templateIds.Length == 0)
            {
                return QuestionAnalyticsBuildResult.Empty();
            }

            var templateQuestions = await _anonymousTemplateQuestionRepository.Query()
                .AsNoTracking()
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
                .AsNoTracking()
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
            IReadOnlyCollection<AnswerFlatDto> answers;

            if (responseIds.Length == 0)
            {
                answers = Array.Empty<AnswerFlatDto>();
            }
            else
            {
                var answerQuery = _anonymousSurveyAnswerRepository.Query()
                    .AsNoTracking()
                    .Where(x => responseIds.Contains(x.AnonymousSurveyResponseId));

                var loadedAnswers = includeResponseDetails
                    ? await answerQuery
                        .Select(x => new AnswerFlatDto
                        {
                            ResponseId = x.AnonymousSurveyResponseId,
                            TemplateQuestionId = x.AnonymousTemplateQuestionId,
                            QuestionId = x.QuestionId,
                            QuestionType = x.QuestionType,
                            SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                            StarRatingValue = x.StarRatingValue,
                            SmileValue = x.SmileValue,
                            HasTextAnswer = !string.IsNullOrWhiteSpace(x.TextAnswer),
                            HasVoiceAnswer = !string.IsNullOrWhiteSpace(x.VoiceFileName),
                            QuestionTextEn = x.Question.TextEn,
                            QuestionTextAr = x.Question.TextAr,
                            TextAnswer = x.TextAnswer,
                            VoiceFileName = x.VoiceFileName,
                            ImageFileName = x.ImageFileName
                        })
                        .ToArrayAsync(cancellationToken)
                    : await answerQuery
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

                answers = AttachTemplateIds(loadedAnswers, responses);
            }

            return await BuildQuestionAnalyticsAsync(
                templates,
                templateQuestions,
                conditions,
                responses,
                answers,
                ReportTemplateKind.Anonymous,
                scoreCalculationMode,
                includeResponseDetails,
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
            bool includeResponseDetails,
            CancellationToken cancellationToken)
        {
            var questionIds = templateQuestions
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            var questionOptions = questionIds.Length == 0
                ? Array.Empty<QuestionOptionFlatDto>()
                : await _questionOptionRepository.Query()
                    .AsNoTracking()
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

            var selectedOptionIds = includeResponseDetails
                ? answers
                    .Where(x => x.SelectedQuestionOptionId.HasValue)
                    .Select(x => x.SelectedQuestionOptionId!.Value)
                    .Distinct()
                    .ToArray()
                : Array.Empty<Guid>();

            var selectedDisplayOptions = selectedOptionIds.Length == 0
                ? Array.Empty<QuestionOptionFlatDto>()
                : await _questionOptionRepository.Query()
                    .AsNoTracking()
                    .Where(x => selectedOptionIds.Contains(x.Id))
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

            var scoreTokens = _surveyReportScoringService.CalculateQuestionScoreTokens(
                responses,
                answers,
                templateQuestions,
                conditions,
                questionOptions,
                scoreCalculationMode);

            var responseScores = _surveyReportScoringService.CalculateResponseScores(
                responses,
                answers,
                templateQuestions,
                conditions,
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
                questionScoreTokens ??= Array.Empty<QuestionScoreToken>();

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
                    : (decimal?)ReportScoreRounding.Round(questionScoreTokens.Average(x => x.ScoreValue));

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
                    QuestionOrder = templateQuestion.Order,
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

            var detailedAnswers = includeResponseDetails
                ? BuildDetailedAnswers(
                    answers,
                    templateQuestions,
                    conditions,
                    questionOptions,
                    selectedDisplayOptions,
                    scoreTokens,
                    templateKind,
                    scoreCalculationMode)
                : Array.Empty<BranchTemplatesReportAnswer>();

            return new QuestionAnalyticsBuildResult(
                result,
                scoreTokens,
                responseScores,
                flowLines,
                detailedAnswers);
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

        private static IReadOnlyCollection<BranchTemplatesReportAnswer> BuildDetailedAnswers(
            IReadOnlyCollection<AnswerFlatDto> answers,
            IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
            IReadOnlyCollection<ConditionFlatDto> conditions,
            IReadOnlyCollection<QuestionOptionFlatDto> activeQuestionOptions,
            IReadOnlyCollection<QuestionOptionFlatDto> selectedDisplayOptions,
            IReadOnlyCollection<QuestionScoreToken> scoreTokens,
            ReportTemplateKind templateKind,
            ScoreCalculationMode scoreCalculationMode)
        {
            var templateQuestionsById = templateQuestions
                .GroupBy(x => x.TemplateQuestionId)
                .ToDictionary(x => x.Key, x => x.First());

            var templateQuestionsByQuestion = templateQuestions
                .GroupBy(x => new { x.TemplateId, x.QuestionId })
                .ToDictionary(
                    x => (x.Key.TemplateId, x.Key.QuestionId),
                    x => x.OrderBy(q => q.Order).First());

            var childTemplateQuestionIds = conditions
                .Select(x => x.ChildTemplateQuestionId)
                .ToHashSet();

            var parentConditionsByChild = conditions
                .GroupBy(x => x.ChildTemplateQuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(condition => condition.TriggerValue ?? 0).First());

            var activeOptionsById = activeQuestionOptions
                .GroupBy(x => x.OptionId)
                .ToDictionary(x => x.Key, x => x.First());

            var displayOptionsById = activeQuestionOptions
                .Concat(selectedDisplayOptions)
                .GroupBy(x => x.OptionId)
                .ToDictionary(x => x.Key, x => x.First());

            var includedTokens = scoreTokens
                .GroupBy(x => new { x.ResponseId, x.TemplateQuestionId })
                .ToDictionary(
                    x => (x.Key.ResponseId, x.Key.TemplateQuestionId),
                    x => x.First());

            var result = new List<BranchTemplatesReportAnswer>();

            foreach (var answer in answers)
            {
                TemplateQuestionFlatDto? templateQuestion = null;

                if (answer.TemplateQuestionId.HasValue)
                {
                    templateQuestionsById.TryGetValue(
                        answer.TemplateQuestionId.Value,
                        out templateQuestion);
                }

                templateQuestion ??= templateQuestionsByQuestion.GetValueOrDefault(
                    (answer.TemplateId, answer.QuestionId));

                QuestionOptionFlatDto? selectedOption = null;

                if (answer.SelectedQuestionOptionId.HasValue)
                {
                    displayOptionsById.TryGetValue(
                        answer.SelectedQuestionOptionId.Value,
                        out selectedOption);
                }

                var templateQuestionId = templateQuestion?.TemplateQuestionId;
                var isRootQuestion = templateQuestionId.HasValue &&
                    !childTemplateQuestionIds.Contains(templateQuestionId.Value);

                ParentTriggerText? parentTrigger = null;

                if (templateQuestionId.HasValue &&
                    parentConditionsByChild.TryGetValue(templateQuestionId.Value, out var parentCondition))
                {
                    parentTrigger = BuildParentTriggerText(parentCondition, displayOptionsById);
                }

                var questionType = answer.QuestionType;
                var includedInScore = templateQuestionId.HasValue &&
                    includedTokens.ContainsKey((answer.ResponseId, templateQuestionId.Value));

                result.Add(new BranchTemplatesReportAnswer
                {
                    ResponseId = answer.ResponseId,
                    TemplateId = answer.TemplateId,
                    TemplateKind = templateKind,
                    TemplateQuestionId = templateQuestionId,
                    QuestionId = answer.QuestionId,
                    QuestionOrder = templateQuestion?.Order,
                    QuestionTextEn = templateQuestion?.QuestionTextEn ?? answer.QuestionTextEn,
                    QuestionTextAr = templateQuestion?.QuestionTextAr ?? answer.QuestionTextAr,
                    QuestionType = questionType,
                    IsRootQuestion = isRootQuestion,
                    ParentTriggerTextEn = parentTrigger?.TextEn,
                    ParentTriggerTextAr = parentTrigger?.TextAr,
                    SelectedQuestionOptionId = answer.SelectedQuestionOptionId,
                    SelectedOptionTextEn = selectedOption?.TextEn,
                    SelectedOptionTextAr = selectedOption?.TextAr,
                    SelectedOptionValue = selectedOption?.Value,
                    StarRatingValue = answer.StarRatingValue,
                    SmileValue = answer.SmileValue,
                    TextAnswer = answer.TextAnswer,
                    VoiceFileName = answer.VoiceFileName,
                    VoiceFilePath = BuildMediaPath(SurveyVoiceAnswersBasePath, answer.VoiceFileName),
                    ImageFileName = answer.ImageFileName,
                    ImageFilePath = BuildMediaPath(SurveyAnswerImagesBasePath, answer.ImageFileName),
                    DisplayValue = ResolveAnswerDisplayValue(answer, selectedOption),
                    IsScorable = IsScoredQuestionType(questionType),
                    ScoreValue = ResolveAnswerScoreValue(answer, questionType, activeOptionsById),
                    IncludedInScore = includedInScore,
                    ScoreInclusionReason = ResolveScoreInclusionReason(
                        questionType,
                        isRootQuestion,
                        includedInScore,
                        scoreCalculationMode)
                });
            }

            return result
                .OrderBy(x => x.ResponseId)
                .ThenBy(x => x.QuestionOrder ?? int.MaxValue)
                .ThenBy(x => x.QuestionTextEn)
                .ToArray();
        }

        private async Task<IReadOnlyCollection<BranchTemplatesReportCustomInputDefinition>> LoadNormalCustomInputDefinitionsAsync(
            IReadOnlyCollection<Guid> templateIds,
            CancellationToken cancellationToken)
        {
            if (templateIds.Count == 0)
            {
                return Array.Empty<BranchTemplatesReportCustomInputDefinition>();
            }

            return await _templateCustomInputRepository.Query()
                .AsNoTracking()
                .Where(x => templateIds.Contains(x.TemplateId) && x.IsActive)
                .Select(x => new BranchTemplatesReportCustomInputDefinition
                {
                    TemplateId = x.TemplateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    CustomInputId = x.Id,
                    Name = x.Name,
                    LabelEn = x.LabelEn,
                    LabelAr = x.LabelAr,
                    Type = x.Type,
                    Order = x.Order
                })
                .ToArrayAsync(cancellationToken);
        }

        private async Task<IReadOnlyCollection<BranchTemplatesReportCustomInputDefinition>> LoadAnonymousCustomInputDefinitionsAsync(
            IReadOnlyCollection<Guid> templateIds,
            CancellationToken cancellationToken)
        {
            if (templateIds.Count == 0)
            {
                return Array.Empty<BranchTemplatesReportCustomInputDefinition>();
            }

            return await _anonymousTemplateCustomInputRepository.Query()
                .AsNoTracking()
                .Where(x => templateIds.Contains(x.AnonymousTemplateId) && x.IsActive)
                .Select(x => new BranchTemplatesReportCustomInputDefinition
                {
                    TemplateId = x.AnonymousTemplateId,
                    TemplateKind = ReportTemplateKind.Anonymous,
                    CustomInputId = x.Id,
                    Name = x.Name,
                    LabelEn = x.LabelEn,
                    LabelAr = x.LabelAr,
                    Type = x.Type,
                    Order = x.Order
                })
                .ToArrayAsync(cancellationToken);
        }

        private async Task<IReadOnlyCollection<BranchTemplatesReportCustomInputValue>> LoadNormalCustomInputValuesAsync(
            IReadOnlyCollection<ResponseFlatDto> responses,
            IReadOnlyCollection<BranchTemplatesReportCustomInputDefinition> definitions,
            CancellationToken cancellationToken)
        {
            var responseIds = responses.Select(x => x.ResponseId).ToArray();

            if (responseIds.Length == 0)
            {
                return Array.Empty<BranchTemplatesReportCustomInputValue>();
            }

            var values = await _surveyResponseCustomInputValueRepository.Query()
                .AsNoTracking()
                .Where(x => responseIds.Contains(x.SurveyResponseId))
                .Select(x => new BranchTemplatesReportCustomInputValue
                {
                    ResponseId = x.SurveyResponseId,
                    TemplateId = x.SurveyResponse.TemplateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    CustomInputId = x.TemplateCustomInputId,
                    Name = x.NameSnapshot,
                    Type = x.TypeSnapshot,
                    StringValue = x.StringValue,
                    IntegerValue = x.IntegerValue
                })
                .ToArrayAsync(cancellationToken);

            var ordersById = definitions.ToDictionary(x => x.CustomInputId, x => x.Order);

            return values
                .Select(x => x with
                {
                    Order = ordersById.TryGetValue(x.CustomInputId, out var order)
                        ? order
                        : null
                })
                .ToArray();
        }

        private async Task<IReadOnlyCollection<BranchTemplatesReportCustomInputValue>> LoadAnonymousCustomInputValuesAsync(
            IReadOnlyCollection<ResponseFlatDto> responses,
            IReadOnlyCollection<BranchTemplatesReportCustomInputDefinition> definitions,
            CancellationToken cancellationToken)
        {
            var responseIds = responses.Select(x => x.ResponseId).ToArray();

            if (responseIds.Length == 0)
            {
                return Array.Empty<BranchTemplatesReportCustomInputValue>();
            }

            var values = await _anonymousSurveyResponseCustomInputValueRepository.Query()
                .AsNoTracking()
                .Where(x => responseIds.Contains(x.AnonymousSurveyResponseId))
                .Select(x => new BranchTemplatesReportCustomInputValue
                {
                    ResponseId = x.AnonymousSurveyResponseId,
                    TemplateId = x.AnonymousSurveyResponse.AnonymousTemplateId,
                    TemplateKind = ReportTemplateKind.Anonymous,
                    CustomInputId = x.AnonymousTemplateCustomInputId,
                    Name = x.NameSnapshot,
                    Type = x.TypeSnapshot,
                    StringValue = x.StringValue,
                    IntegerValue = x.IntegerValue
                })
                .ToArrayAsync(cancellationToken);

            var ordersById = definitions.ToDictionary(x => x.CustomInputId, x => x.Order);

            return values
                .Select(x => x with
                {
                    Order = ordersById.TryGetValue(x.CustomInputId, out var order)
                        ? order
                        : null
                })
                .ToArray();
        }

        private static IReadOnlyCollection<BranchTemplatesReportResponse> BuildReportResponses(
            IReadOnlyCollection<TemplateHeaderDto> normalTemplates,
            IReadOnlyCollection<TemplateHeaderDto> anonymousTemplates,
            IReadOnlyCollection<ResponseFlatDto> normalResponses,
            IReadOnlyCollection<ResponseFlatDto> anonymousResponses,
            IReadOnlyCollection<CalculatedResponseScore> responseScores,
            IReadOnlyCollection<BranchTemplatesReportAnswer> answers,
            IReadOnlyCollection<BranchTemplatesReportCustomInputValue> customInputs)
        {
            var templatesByKey = normalTemplates
                .Concat(anonymousTemplates)
                .ToDictionary(x => (x.TemplateKind, x.TemplateId));

            var scoresByResponse = responseScores
                .GroupBy(x => new { x.TemplateKind, x.ResponseId })
                .ToDictionary(
                    x => (x.Key.TemplateKind, x.Key.ResponseId),
                    x => x.First());

            var answersByResponse = answers
                .GroupBy(x => new { x.TemplateKind, x.ResponseId })
                .ToDictionary(
                    x => (x.Key.TemplateKind, x.Key.ResponseId),
                    x => x.OrderBy(answer => answer.QuestionOrder ?? int.MaxValue).ToArray());

            var customInputsByResponse = customInputs
                .GroupBy(x => new { x.TemplateKind, x.ResponseId })
                .ToDictionary(
                    x => (x.Key.TemplateKind, x.Key.ResponseId),
                    x => x.OrderBy(value => value.Order ?? int.MaxValue).ThenBy(value => value.Name).ToArray());

            return normalResponses
                .Concat(anonymousResponses)
                .OrderBy(x => x.SubmittedOnUtc)
                .ThenBy(x => x.ResponseId)
                .Select(response =>
                {
                    templatesByKey.TryGetValue(
                        (response.TemplateKind, response.TemplateId),
                        out var template);
                    scoresByResponse.TryGetValue(
                        (response.TemplateKind, response.ResponseId),
                        out var score);
                    answersByResponse.TryGetValue(
                        (response.TemplateKind, response.ResponseId),
                        out var responseAnswers);
                    customInputsByResponse.TryGetValue(
                        (response.TemplateKind, response.ResponseId),
                        out var responseCustomInputs);

                    return new BranchTemplatesReportResponse
                    {
                        ResponseId = response.ResponseId,
                        TemplateId = response.TemplateId,
                        TemplateNameEn = template?.NameEn ?? string.Empty,
                        TemplateNameAr = template?.NameAr,
                        TemplateKind = response.TemplateKind,
                        SubmittedOnUtc = response.SubmittedOnUtc,
                        OperatorId = response.OperatorId,
                        OperatorNameEn = response.OperatorNameEn,
                        OperatorNameAr = response.OperatorNameAr,
                        IsScored = score?.HasScore == true,
                        ScoredItemsCount = score?.ScoredItemsCount ?? 0,
                        AverageScoreValue = score?.AverageScoreValue,
                        ScorePercentage = score?.ScorePercentage,
                        Answers = responseAnswers ?? Array.Empty<BranchTemplatesReportAnswer>(),
                        CustomInputs = responseCustomInputs ?? Array.Empty<BranchTemplatesReportCustomInputValue>()
                    };
                })
                .ToArray();
        }

        private static string ResolveAnswerDisplayValue(
            AnswerFlatDto answer,
            QuestionOptionFlatDto? selectedOption)
            => answer.QuestionType switch
            {
                QuestionType.SingleChoice => selectedOption?.TextEn ?? string.Empty,
                QuestionType.StarRating => answer.StarRatingValue?.ToString() ?? string.Empty,
                QuestionType.Smiles => answer.SmileValue?.ToString() ?? string.Empty,
                QuestionType.Complain => answer.TextAnswer ?? string.Empty,
                QuestionType.Voice => BuildMediaPath(SurveyVoiceAnswersBasePath, answer.VoiceFileName) ?? string.Empty,
                QuestionType.Image => BuildMediaPath(SurveyAnswerImagesBasePath, answer.ImageFileName) ?? string.Empty,
                _ => string.Empty
            };

        private static decimal? ResolveAnswerScoreValue(
            AnswerFlatDto answer,
            QuestionType questionType,
            IReadOnlyDictionary<Guid, QuestionOptionFlatDto> activeOptionsById)
        {
            if (questionType == QuestionType.StarRating && answer.StarRatingValue.HasValue)
            {
                return answer.StarRatingValue.Value;
            }

            if (questionType == QuestionType.Smiles && answer.SmileValue.HasValue)
            {
                return answer.SmileValue.Value;
            }

            if (questionType == QuestionType.SingleChoice &&
                answer.SelectedQuestionOptionId.HasValue &&
                activeOptionsById.TryGetValue(answer.SelectedQuestionOptionId.Value, out var option))
            {
                return option.Value;
            }

            return null;
        }

        private static string ResolveScoreInclusionReason(
            QuestionType questionType,
            bool isRootQuestion,
            bool includedInScore,
            ScoreCalculationMode scoreCalculationMode)
        {
            if (!IsScoredQuestionType(questionType))
            {
                return "Non-Scorable Question";
            }

            if (includedInScore)
            {
                return scoreCalculationMode == ScoreCalculationMode.RootQuestions
                    ? "Root Question"
                    : "Selected By Lowest Condition Level";
            }

            return isRootQuestion
                ? "Root Question - Not Included"
                : "Conditional Question - Not Included";
        }

        private static string? BuildMediaPath(string basePath, string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var value = fileName.Trim().Replace('\\', '/');

            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return value;
            }

            if (value.StartsWith("Media/", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            return $"{basePath}/{value.TrimStart('/')}";
        }

        private static IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> BuildTemplateSummaries(
            IReadOnlyCollection<TemplateHeaderDto> normalTemplates,
            IReadOnlyCollection<TemplateHeaderDto> anonymousTemplates,
            IReadOnlyCollection<ResponseFlatDto> normalResponses,
            IReadOnlyCollection<ResponseFlatDto> anonymousResponses,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> normalQuestions,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> anonymousQuestions,
            IReadOnlyCollection<QuestionScoreToken> normalScoreTokens,
            IReadOnlyCollection<QuestionScoreToken> anonymousScoreTokens,
            IReadOnlyCollection<CalculatedResponseScore> normalResponseScores,
            IReadOnlyCollection<CalculatedResponseScore> anonymousResponseScores)
        {
            var summaries = new List<BranchTemplatesPdfTemplateSummary>();

            summaries.AddRange(BuildTemplateSummariesForKind(
                normalTemplates,
                normalResponses,
                normalQuestions,
                normalScoreTokens,
                normalResponseScores,
                ReportTemplateKind.Normal));

            summaries.AddRange(BuildTemplateSummariesForKind(
                anonymousTemplates,
                anonymousResponses,
                anonymousQuestions,
                anonymousScoreTokens,
                anonymousResponseScores,
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
            IReadOnlyCollection<QuestionScoreToken> scoreTokens,
            IReadOnlyCollection<CalculatedResponseScore> responseScores,
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

            var responseScoresByTemplate = responseScores
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            return templates.Select(template =>
            {
                responsesByTemplate.TryGetValue(template.TemplateId, out var templateResponses);
                templateResponses ??= Array.Empty<ResponseFlatDto>();

                questionsByTemplate.TryGetValue(template.TemplateId, out var templateQuestions);
                templateQuestions ??= Array.Empty<BranchTemplatesPdfQuestionAnalytics>();

                scoreTokensByTemplate.TryGetValue(template.TemplateId, out var templateScoreTokens);
                templateScoreTokens ??= Array.Empty<QuestionScoreToken>();

                responseScoresByTemplate.TryGetValue(template.TemplateId, out var templateResponseScores);
                templateResponseScores ??= Array.Empty<CalculatedResponseScore>();

                var avgPercentage = templateResponseScores.Length == 0
                    ? null
                    : (decimal?)ReportScoreRounding.Round(templateResponseScores.Average(x => x.ScorePercentage));

                var avgValue = avgPercentage.HasValue
                    ? (decimal?)ReportScoreRounding.Round(avgPercentage.Value * MaxScoreValue / 100m)
                    : null;

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
                    AverageScorePercentage = avgPercentage
                };
            }).ToArray();
        }

        private static BranchTemplatesPdfExecutiveSummary BuildExecutiveSummary(
            IReadOnlyCollection<BranchTemplatesPdfTemplateSummary> templates,
            IReadOnlyCollection<ResponseFlatDto> normalResponses,
            IReadOnlyCollection<ResponseFlatDto> anonymousResponses,
            IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> questions,
            IReadOnlyCollection<QuestionScoreToken> scoreTokens,
            IReadOnlyCollection<CalculatedResponseScore> responseScores,
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

            var avgPercentage = responseScores.Count == 0
                ? null
                : (decimal?)ReportScoreRounding.Round(responseScores.Average(x => x.ScorePercentage));

            var avgValue = avgPercentage.HasValue
                ? (decimal?)ReportScoreRounding.Round(avgPercentage.Value * MaxScoreValue / 100m)
                : null;

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
                AverageScorePercentage = avgPercentage,
                HighestRatedTemplateName = highest?.DisplayName(isArabic) ?? "-",
                LowestRatedTemplateName = lowest?.DisplayName(isArabic) ?? "-",
                MostAnsweredTemplateName = mostAnswered?.DisplayName(isArabic) ?? "-",
                TemplatesWithoutResponses = templates.Count(x => x.TotalResponses == 0)
            };
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
            IReadOnlyCollection<QuestionScoreToken> ScoreTokens,
            IReadOnlyCollection<CalculatedResponseScore> ResponseScores,
            IReadOnlyCollection<BranchTemplatesPdfFlowLine> FlowLines,
            IReadOnlyCollection<BranchTemplatesReportAnswer> Answers)
        {
            public static QuestionAnalyticsBuildResult Empty()
                => new(
                    Array.Empty<BranchTemplatesPdfQuestionAnalytics>(),
                    Array.Empty<QuestionScoreToken>(),
                    Array.Empty<CalculatedResponseScore>(),
                    Array.Empty<BranchTemplatesPdfFlowLine>(),
                    Array.Empty<BranchTemplatesReportAnswer>());
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

        private sealed record ParentTriggerText(
            string TextEn,
            string TextAr);
    }
}
