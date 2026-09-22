using BuildingBlock.Domain.Results;
using ClosedXML.Excel;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.infrastructure.Reports;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class BranchTemplateExcelReportServiceTests
{
    private static readonly string[] ExpectedSheetNames =
    {
        "01 - Summary",
        "02 - Graphics",
        "03 - Response Matrix",
        "04 - Responses",
        "05 - Answers",
        "06 - Custom Inputs",
        "07 - Question Analysis",
        "08 - Worst Questions",
        "09 - Best Questions"
    };

    [Fact]
    public async Task GenerateAsync_CreatesValidAnalyticalWorkbookWithTypedDetails()
    {
        var model = CreatePopulatedModel();
        var source = new StubBranchTemplatesReportService(model);
        var sut = new BranchTemplateExcelReportService(source);

        var result = await sut.GenerateAsync(CreateRequest(model.SelectedTemplateId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Content);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            result.Value.ContentType);
        Assert.True(source.LastRequest?.IncludeResponseDetails);

        var tempPath = Path.Combine(Path.GetTempPath(), $"customer-survey-{Guid.NewGuid():N}.xlsx");

        try
        {
            await File.WriteAllBytesAsync(tempPath, result.Value.Content);

            using var workbook = new XLWorkbook(tempPath);

            Assert.Equal(ExpectedSheetNames, workbook.Worksheets.Select(x => x.Name));
            Assert.Equal("Customer Satisfaction", workbook.Worksheet("01 - Summary").Cell("B5").GetString());

            var graphics = workbook.Worksheet("02 - Graphics");
            Assert.Equal(5, graphics.Pictures.Count);
            Assert.Equal(
                XLColor.FromHtml("#16A34A").Color.ToArgb(),
                graphics.Cell(6, 14).Style.Fill.BackgroundColor.Color.ToArgb());
            Assert.Equal(
                XLColor.FromHtml("#94A3B8").Color.ToArgb(),
                graphics.Cell(7, 14).Style.Fill.BackgroundColor.Color.ToArgb());

            var matrix = workbook.Worksheet("03 - Response Matrix");
            Assert.Equal("Response No", matrix.Cell(3, 1).GetString());
            Assert.Equal("01012345678", matrix.Cell(4, 7).GetString());
            Assert.Equal("@", matrix.Cell(4, 7).Style.NumberFormat.Format);
            Assert.Equal(1d, matrix.Cell(4, 6).GetDouble());
            Assert.True(matrix.Cell(5, 6).IsEmpty());
            Assert.True(matrix.Column(matrix.LastColumnUsed()!.ColumnNumber()).IsHidden);

            var responses = workbook.Worksheet("04 - Responses");
            var answers = workbook.Worksheet("05 - Answers");
            var customInputs = workbook.Worksheet("06 - Custom Inputs");
            Assert.Equal("Rate the service", answers.Cell(4, 4).GetString());
            Assert.True(answers.Cell(4, 7).IsEmpty());
            Assert.Equal("Yes", answers.Cell(4, 11).GetString());
            Assert.Equal("No", answers.Cell(5, 11).GetString());
            Assert.Equal("Root Question", answers.Cell(4, 12).GetString());
            Assert.Equal(matrix.Cell(4, 1).GetDouble(), responses.Cell(4, 1).GetDouble());
            Assert.Equal(matrix.Cell(4, 1).GetDouble(), answers.Cell(4, 1).GetDouble());
            Assert.Equal(matrix.Cell(4, 1).GetDouble(), customInputs.Cell(4, 1).GetDouble());
            Assert.True(responses.Column(7).IsHidden);
            Assert.True(answers.Column(14).IsHidden);
            Assert.True(customInputs.Column(6).IsHidden);

            var matrixHeaders = matrix.Row(3).CellsUsed().Select(x => x.GetString()).ToArray();
            Assert.DoesNotContain("Template Kind", matrixHeaders);
            Assert.DoesNotContain("Operator Id", matrixHeaders);
            Assert.DoesNotContain("Max Score", matrixHeaders);

            var responseHeaders = responses.Row(3).CellsUsed().Select(x => x.GetString()).ToArray();
            Assert.DoesNotContain("Template Id", responseHeaders);
            Assert.DoesNotContain("Template Name", responseHeaders);
            Assert.DoesNotContain("Scored Items Count", responseHeaders);

            var answerHeaders = answers.Row(3).CellsUsed().Select(x => x.GetString()).ToArray();
            Assert.DoesNotContain("Template Question Id", answerHeaders);
            Assert.DoesNotContain("Selected Option Id", answerHeaders);
            Assert.DoesNotContain("Star Rating", answerHeaders);

            Assert.NotEmpty(matrix.Tables);
            Assert.NotEmpty(responses.Tables);
            Assert.NotEmpty(answers.Tables);

            var questionAnalysis = workbook.Worksheet("07 - Question Analysis");
            Assert.Equal("Yes", questionAnalysis.Cell(4, 10).GetString());
            Assert.Equal("No", questionAnalysis.Cell(5, 10).GetString());

            var summaryMetricLabels = workbook.Worksheet("01 - Summary")
                .Range("A17:A27")
                .Cells()
                .Select(cell => cell.GetString());
            Assert.DoesNotContain("Template Status", summaryMetricLabels);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public async Task GenerateAsync_EmptyPeriodCreatesAllSheetsWithHeadersAndNoFakeRows()
    {
        var templateId = Guid.NewGuid();
        var model = CreateBaseModel(templateId) with
        {
            Templates = new[]
            {
                CreateTemplateSummary(templateId) with
                {
                    TotalResponses = 0,
                    TotalAnswers = 0,
                    TotalScoredAnswers = 0
                }
            }
        };
        var sut = new BranchTemplateExcelReportService(
            new StubBranchTemplatesReportService(model));

        var result = await sut.GenerateAsync(CreateRequest(templateId), CancellationToken.None);

        Assert.True(result.IsSuccess);

        using var stream = new MemoryStream(result.Value.Content);
        using var workbook = new XLWorkbook(stream);

        Assert.Equal(ExpectedSheetNames, workbook.Worksheets.Select(x => x.Name));
        var responses = workbook.Worksheet("04 - Responses");
        Assert.Equal("Survey Response Id", responses.Cell(3, 7).GetString());
        Assert.True(responses.Column(7).IsHidden);
        Assert.True(responses.Cell(4, 1).IsEmpty());
        Assert.True(responses.AutoFilter.IsEnabled);
    }

    [Fact]
    public async Task GenerateAsync_RejectsMissingTemplateIdBeforeBuildingDataset()
    {
        var source = new StubBranchTemplatesReportService(CreateBaseModel(Guid.NewGuid()));
        var sut = new BranchTemplateExcelReportService(source);

        var result = await sut.GenerateAsync(CreateRequest(null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Null(source.LastRequest);
    }

    [Fact]
    public async Task GenerateAsync_CreatesHyperlinkOnlyForAbsoluteMediaUrl()
    {
        var model = CreatePopulatedModel();
        var firstResponse = model.Responses.First();
        var voice = new BranchTemplatesReportAnswer
        {
            ResponseId = firstResponse.ResponseId,
            TemplateId = firstResponse.TemplateId,
            TemplateKind = firstResponse.TemplateKind,
            QuestionId = Guid.NewGuid(),
            QuestionTextEn = "Voice feedback",
            QuestionType = QuestionType.Voice,
            VoiceFilePath = "https://cdn.example.com/voice.mp3",
            DisplayValue = "https://cdn.example.com/voice.mp3",
            ScoreInclusionReason = "Non-Scorable Question"
        };
        var image = voice with
        {
            QuestionId = Guid.NewGuid(),
            QuestionTextEn = "Receipt image",
            QuestionType = QuestionType.Image,
            VoiceFilePath = null,
            ImageFilePath = "Media/SurveyAnswerImages/receipt.png",
            DisplayValue = "Media/SurveyAnswerImages/receipt.png"
        };
        model = model with
        {
            Responses = model.Responses
                .Select(x => x.ResponseId == firstResponse.ResponseId
                    ? x with { Answers = x.Answers.Concat(new[] { voice, image }).ToArray() }
                    : x)
                .ToArray()
        };

        var sut = new BranchTemplateExcelReportService(new StubBranchTemplatesReportService(model));
        var result = await sut.GenerateAsync(CreateRequest(model.SelectedTemplateId), CancellationToken.None);

        using var stream = new MemoryStream(result.Value.Content);
        using var workbook = new XLWorkbook(stream);
        var answers = workbook.Worksheet("05 - Answers");

        Assert.Equal("Open Voice", answers.Cell(5, 13).GetString());
        Assert.Equal("https://cdn.example.com/voice.mp3", answers.Cell(5, 13).GetHyperlink().ExternalAddress.ToString());
        Assert.Equal("Media/SurveyAnswerImages/receipt.png", answers.Cell(6, 13).GetString());
        Assert.False(answers.Cell(6, 13).HasHyperlink);
    }

    [Fact]
    public async Task GenerateAsync_ArabicReportLocalizesVisibleWorkbookLabels()
    {
        var model = CreatePopulatedModel() with
        {
            Language = "ar"
        };
        var sut = new BranchTemplateExcelReportService(new StubBranchTemplatesReportService(model));

        var result = await sut.GenerateAsync(
            CreateRequest(model.SelectedTemplateId) with { Language = "ar" },
            CancellationToken.None);

        using var stream = new MemoryStream(result.Value.Content);
        using var workbook = new XLWorkbook(stream);

        Assert.Equal("تقرير نموذج استبيان العملاء", workbook.Worksheet("01 - Summary").Cell("A1").GetString());
        Assert.Equal("التحليلات المرئية", workbook.Worksheet("02 - Graphics").Cell("A1").GetString());
        Assert.Equal("رقم الرد", workbook.Worksheet("03 - Response Matrix").Cell(3, 1).GetString());
        Assert.True(workbook.Worksheet("03 - Response Matrix").RightToLeft);

        foreach (var sheetName in new[]
                 {
                     "03 - Response Matrix",
                     "04 - Responses",
                     "05 - Answers",
                     "06 - Custom Inputs"
                 })
        {
            var worksheet = workbook.Worksheet(sheetName);
            Assert.Equal(XLDataType.DateTime, worksheet.Cell(4, 2).DataType);
            Assert.Equal("yyyy-mm-dd hh:mm:ss", worksheet.Cell(4, 2).Style.DateFormat.Format);
            Assert.True(worksheet.Column(2).Width >= 22);
        }

        var answers = workbook.Worksheet("05 - Answers");
        Assert.Equal("نعم", answers.Cell(4, 11).GetString());
        Assert.Equal("لا", answers.Cell(5, 11).GetString());

        var questionAnalysis = workbook.Worksheet("07 - Question Analysis");
        Assert.Equal("نعم", questionAnalysis.Cell(4, 10).GetString());
        Assert.Equal("لا", questionAnalysis.Cell(5, 10).GetString());

        var summaryMetricLabels = workbook.Worksheet("01 - Summary")
            .Range("A17:A27")
            .Cells()
            .Select(cell => cell.GetString());
        Assert.DoesNotContain("حالة النموذج", summaryMetricLabels);
    }

    private static BranchTemplatesPdfReportModel CreatePopulatedModel()
    {
        var templateId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var templateQuestionId = Guid.NewGuid();
        var customInputId = Guid.NewGuid();
        var scoredResponseId = Guid.NewGuid();
        var unscoredResponseId = Guid.NewGuid();

        var question = new BranchTemplatesPdfQuestionAnalytics
        {
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateNameEn = "Customer Satisfaction",
            TemplateQuestionId = templateQuestionId,
            QuestionId = questionId,
            QuestionOrder = 1,
            QuestionTextEn = "Rate the service",
            QuestionType = QuestionType.StarRating.ToString(),
            IsRootQuestion = true,
            TotalAnswers = 1,
            SkippedCount = 1,
            AverageValue = 5m,
            ScoreAverageValue = 5m,
            IsScoreIncluded = true
        };

        var rank = new BranchTemplatesPdfQuestionRankItem
        {
            Rank = 1,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateNameEn = "Customer Satisfaction",
            TemplateQuestionId = templateQuestionId,
            QuestionTextEn = "Rate the service",
            IsRootQuestion = true,
            QuestionType = QuestionType.StarRating.ToString(),
            TotalAnswers = 1,
            AverageScoreValue = 5m,
            SatisfactionPercentage = 100m
        };

        var scoredAnswer = new BranchTemplatesReportAnswer
        {
            ResponseId = scoredResponseId,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateQuestionId = templateQuestionId,
            QuestionId = questionId,
            QuestionOrder = 1,
            QuestionTextEn = "Rate the service",
            QuestionType = QuestionType.StarRating,
            IsRootQuestion = true,
            StarRatingValue = 5,
            DisplayValue = "5",
            IsScorable = true,
            ScoreValue = 5m,
            IncludedInScore = true,
            ScoreInclusionReason = "Root Question"
        };

        var nonScoredQuestionId = Guid.NewGuid();
        var nonScoredTemplateQuestionId = Guid.NewGuid();
        var nonScoredQuestion = new BranchTemplatesPdfQuestionAnalytics
        {
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateNameEn = "Customer Satisfaction",
            TemplateQuestionId = nonScoredTemplateQuestionId,
            QuestionId = nonScoredQuestionId,
            QuestionOrder = 2,
            QuestionTextEn = "Additional feedback",
            QuestionType = QuestionType.Complain.ToString(),
            IsRootQuestion = true,
            TotalAnswers = 1,
            IsScoreIncluded = false
        };

        var nonScoredAnswer = new BranchTemplatesReportAnswer
        {
            ResponseId = unscoredResponseId,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateQuestionId = nonScoredTemplateQuestionId,
            QuestionId = nonScoredQuestionId,
            QuestionOrder = 2,
            QuestionTextEn = "Additional feedback",
            QuestionType = QuestionType.Complain,
            IsRootQuestion = true,
            TextAnswer = "No comment",
            DisplayValue = "No comment",
            IncludedInScore = false,
            ScoreInclusionReason = "Non-Scorable Question"
        };

        var phoneValue = new BranchTemplatesReportCustomInputValue
        {
            ResponseId = scoredResponseId,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            CustomInputId = customInputId,
            Name = "Phone",
            Type = TemplateCustomInputType.String,
            Order = 1,
            StringValue = "01012345678"
        };

        return CreateBaseModel(templateId) with
        {
            ExecutiveSummary = new BranchTemplatesPdfExecutiveSummary
            {
                TotalNormalTemplates = 1,
                TotalResponses = 2,
                TotalNormalResponses = 2,
                TotalAnswers = 2,
                TotalScoredAnswers = 1,
                TotalNonScoredAnswers = 1,
                AverageScoreValue = 5m,
                AverageScorePercentage = 100m
            },
            Graphics = new BranchTemplatesReportGraphics
            {
                OverallSatisfactionPercentage = 100m,
                AverageScoreValue = 5m,
                TotalResponses = 2,
                ScoredResponses = 1,
                NotScoredResponses = 1,
                ExcellentResponses = 1,
                RootQuestions = 1,
                IncludedAnswers = 1,
                NonScoredAnswers = 1
            },
            Templates = new[]
            {
                CreateTemplateSummary(templateId) with
                {
                    TotalResponses = 2,
                    TotalAnswers = 2,
                    TotalScoredAnswers = 1,
                    AverageScoreValue = 5m,
                    AverageScorePercentage = 100m
                }
            },
            Questions = new[] { question, nonScoredQuestion },
            WorstQuestions = new[] { rank },
            BestQuestions = new[] { rank },
            CustomInputDefinitions = new[]
            {
                new BranchTemplatesReportCustomInputDefinition
                {
                    TemplateId = templateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    CustomInputId = customInputId,
                    Name = "Phone",
                    LabelEn = "Phone",
                    Type = TemplateCustomInputType.String,
                    Order = 1
                }
            },
            Responses = new[]
            {
                new BranchTemplatesReportResponse
                {
                    ResponseId = scoredResponseId,
                    TemplateId = templateId,
                    TemplateNameEn = "Customer Satisfaction",
                    TemplateKind = ReportTemplateKind.Normal,
                    SubmittedOnUtc = new DateTime(2026, 9, 10, 10, 30, 0, DateTimeKind.Utc),
                    OperatorId = Guid.NewGuid(),
                    OperatorNameEn = "Operator One",
                    IsScored = true,
                    ScoredItemsCount = 1,
                    AverageScoreValue = 5m,
                    ScorePercentage = 100m,
                    CustomInputs = new[] { phoneValue },
                    Answers = new[] { scoredAnswer }
                },
                new BranchTemplatesReportResponse
                {
                    ResponseId = unscoredResponseId,
                    TemplateId = templateId,
                    TemplateNameEn = "Customer Satisfaction",
                    TemplateKind = ReportTemplateKind.Normal,
                    SubmittedOnUtc = new DateTime(2026, 9, 11, 11, 0, 0, DateTimeKind.Utc),
                    OperatorId = Guid.NewGuid(),
                    OperatorNameEn = "Operator Two",
                    IsScored = false,
                    Answers = new[] { nonScoredAnswer }
                }
            }
        };
    }

    private static BranchTemplatesPdfReportModel CreateBaseModel(Guid templateId)
        => new()
        {
            Language = "en",
            BranchName = "Cairo Branch",
            GeneratedBy = "Report User",
            GeneratedAtUtc = new DateTime(2026, 9, 21, 15, 30, 0, DateTimeKind.Utc),
            FromDate = new DateOnly(2026, 9, 1),
            ToDate = new DateOnly(2026, 9, 21),
            SelectedTemplateId = templateId,
            SelectedTemplateName = "Customer Satisfaction",
            SelectedTemplateKind = ReportTemplateKind.Normal,
            ScoreCalculationMode = ScoreCalculationMode.RootQuestions,
            TopWorstQuestionsCount = 10,
            WorstQuestionsMaxScorePercentage = 40m,
            BestQuestionsMinScorePercentage = 70m,
            Templates = new[] { CreateTemplateSummary(templateId) }
        };

    private static BranchTemplatesPdfTemplateSummary CreateTemplateSummary(Guid templateId)
        => new()
        {
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            NameEn = "Customer Satisfaction",
            Status = "Active",
            TotalQuestions = 1,
            RootQuestions = 1
        };

    private static BranchTemplatesPdfReportRequest CreateRequest(Guid? templateId)
        => new()
        {
            BranchId = Guid.NewGuid(),
            GeneratedByApplicationUserId = Guid.NewGuid(),
            GeneratedByName = "Report User",
            TemplateId = templateId,
            FromDate = new DateOnly(2026, 9, 1),
            ToDate = new DateOnly(2026, 9, 21),
            ScoreCalculationMode = ScoreCalculationMode.RootQuestions,
            TopWorstQuestionsCount = 10,
            WorstQuestionsMaxScorePercentage = 40m,
            BestQuestionsMinScorePercentage = 70m,
            Language = "en"
        };

    private sealed class StubBranchTemplatesReportService : IBranchTemplatesPdfReportService
    {
        private readonly BranchTemplatesPdfReportModel _model;

        public StubBranchTemplatesReportService(BranchTemplatesPdfReportModel model)
        {
            _model = model;
        }

        public BranchTemplatesPdfReportRequest? LastRequest { get; private set; }

        public Task<Result<BranchTemplatesPdfReportModel>> BuildReportModelAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(Result<BranchTemplatesPdfReportModel>.Ok(_model));
        }

        public Task<Result<BranchTemplatesPdfReportFile>> GenerateAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
